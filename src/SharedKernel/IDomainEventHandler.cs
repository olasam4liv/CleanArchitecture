namespace SharedKernel;

public interface IDomainEventProcessor<in T> where T : IDomainEvent
{
    Task Handle(T domainEvent, CancellationToken cancellationToken);
}
