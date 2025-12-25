using Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Outbox;

internal sealed class OutboxProcessor(
    IServiceProvider serviceProvider,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 50;
    private const int MaxAttempts = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = serviceProvider.CreateScope();
                Database.ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<Database.ApplicationDbContext>();
                IEventBus bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                List<OutboxMessage> pending = await db.OutboxMessages
                    .Where(m => m.ProcessedOnUtc == null && m.Attempt < MaxAttempts)
                    .OrderBy(m => m.OccurredOnUtc)
                    .Take(BatchSize)
                    .ToListAsync(stoppingToken);

                if (pending.Count == 0)
                {
                    await Task.Delay(PollInterval, stoppingToken);
                    continue;
                }

                foreach (OutboxMessage message in pending)
                {
                    try
                    {
                        await bus.PublishAsync(message.Type, message.Content, stoppingToken);
                        message.ProcessedOnUtc = DateTime.UtcNow;
                        message.Error = null;
                    }
                    catch (Exception ex)
                    {
                        message.Attempt += 1;
                        message.Error = ex.Message;
                        logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OutboxProcessor loop error");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
