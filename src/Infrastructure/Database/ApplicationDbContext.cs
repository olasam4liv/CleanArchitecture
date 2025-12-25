using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Todos;
using Domain.Users;
using Infrastructure.DomainEvents;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System.Text.Json;
using Infrastructure.Outbox;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher,
    IEnumerable<IIntegrationEventMapper> integrationEventMappers)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<TodoItem> TodoItems { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Extract and clear domain events before saving, so we can
        // persist Outbox messages in the same transaction (transactional outbox).
        List<IDomainEvent> domainEvents = ExtractDomainEvents();

        if (domainEvents.Count > 0)
        {
            foreach (IDomainEvent domainEvent in domainEvents)
            {
                // Try to map domain event to integration event
                IntegrationEvent? integrationEvent = MapToIntegrationEvent(domainEvent);

                var message = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccurredOnUtc = DateTime.UtcNow,
                    Type = integrationEvent?.GetType().FullName ?? domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                    Content = JsonSerializer.Serialize(
                        integrationEvent ?? (object)domainEvent,
                        integrationEvent?.GetType() ?? domainEvent.GetType())
                };

                OutboxMessages.Add(message);
            }
        }

        int result = await base.SaveChangesAsync(cancellationToken);

        // In-process handlers still execute after commit to avoid blocking the transaction.
        if (domainEvents.Count > 0)
        {
            await domainEventsDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    private List<IDomainEvent> ExtractDomainEvents()
    {
        return ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return events;
            })
            .ToList();
    }

    private IntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        foreach (IIntegrationEventMapper mapper in integrationEventMappers)
        {
            IntegrationEvent? integrationEvent = mapper.Map(domainEvent);
            if (integrationEvent != null)
            {
                return integrationEvent;
            }
        }

        return null;
    }
}
