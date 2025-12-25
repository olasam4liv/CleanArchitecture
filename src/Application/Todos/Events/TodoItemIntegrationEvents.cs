namespace Application.Todos.Events;

/// <summary>
/// Integration event published when a todo item is created.
/// </summary>
public sealed record TodoItemCreatedIntegrationEvent : Application.Abstractions.Messaging.IntegrationEvent
{
    public Guid TodoItemId { get; init; }
    public Guid UserId { get; init; }
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Integration event published when a todo item is completed.
/// </summary>
public sealed record TodoItemCompletedIntegrationEvent : Application.Abstractions.Messaging.IntegrationEvent
{
    public Guid TodoItemId { get; init; }
    public Guid UserId { get; init; }
}

/// <summary>
/// Integration event published when a todo item is deleted.
/// </summary>
public sealed record TodoItemDeletedIntegrationEvent : Application.Abstractions.Messaging.IntegrationEvent
{
    public Guid TodoItemId { get; init; }
    public Guid UserId { get; init; }
}
