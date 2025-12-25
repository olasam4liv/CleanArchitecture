using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging;

internal sealed class InMemoryEventBus(ILogger<InMemoryEventBus> logger) : IEventBus
{
    public Task PublishAsync(string type, string content, CancellationToken cancellationToken = default)
    {
        // Minimal placeholder publisher. Replace with MassTransit or other broker.
        logger.LogInformation("Published integration event: {Type} | Payload: {Payload}", type, content);
        return Task.CompletedTask;
    }
}
