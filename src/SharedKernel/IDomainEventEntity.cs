namespace SharedKernel;

public interface IDomainEventEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void RaiseDomainEvent(IDomainEvent domainEvent);

    void ClearDomainEvents();
}
