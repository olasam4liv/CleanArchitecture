using Application.Abstractions.Messaging;
using Domain.Users;

namespace Application.Users.Events;

/// <summary>
/// Maps user domain events to integration events.
/// </summary>
internal sealed class UserIntegrationEventMapper : IIntegrationEventMapper
{
    public IntegrationEvent? Map(object domainEvent)
    {
        return domainEvent switch
        {
            Domain.Users.UserRegisteredDomainEvent evt => new UserRegisteredIntegrationEvent
            {
                EventType = nameof(UserRegisteredIntegrationEvent),
                UserId = evt.UserId,
                Email = string.Empty,  // TODO: enrich from User entity if needed
                FirstName = string.Empty,
                LastName = string.Empty
            },
            _ => null
        };
    }
}
