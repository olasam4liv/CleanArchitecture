namespace Application.Users.Events;

/// <summary>
/// Integration event published when a user registers.
/// </summary>
public sealed record UserRegisteredIntegrationEvent : Application.Abstractions.Messaging.IntegrationEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}
