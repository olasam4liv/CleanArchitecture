using SharedKernel;

namespace Domain.DomainEvents.Todo;

public sealed record TodoItemCompletedDomainEvent(Guid TodoItemId) : IDomainEvent;
