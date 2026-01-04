using SharedKernel;

namespace Domain.DomainEvents.User;

public sealed record UserRegisteredDomainEvent(Guid UserId) : IDomainEvent;
