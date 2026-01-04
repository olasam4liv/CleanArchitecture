using SharedKernel;

namespace Domain.DomainEvents.Todo;

public sealed record TodoItemCreatedDomainEvent(Guid TodoItemId) : IDomainEvent;
