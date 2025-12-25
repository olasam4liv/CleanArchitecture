namespace Application.Abstractions.Messaging;

/// <summary>
/// Base contract for integration events published to external systems/brokers.
/// Integration events are cross-service contracts; they may differ from domain events.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    public string EventType { get; init; } = string.Empty;
}
