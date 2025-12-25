namespace Application.Abstractions.Messaging;

/// <summary>
/// Maps domain events to integration events for publication to external systems.
/// </summary>
public interface IIntegrationEventMapper
{
    IntegrationEvent? Map(object domainEvent);
}
