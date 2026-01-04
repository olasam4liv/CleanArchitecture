using SharedKernel;

namespace Domain.DomainEvents.Todo;

public sealed record TodoItemDeletedDomainEvent(Guid TodoItemId) : IDomainEvent;
