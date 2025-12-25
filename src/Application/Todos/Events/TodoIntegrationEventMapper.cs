using Application.Abstractions.Messaging;
using Domain.Todos;

namespace Application.Todos.Events;

/// <summary>
/// Maps todo domain events to integration events.
/// </summary>
internal sealed class TodoIntegrationEventMapper : IIntegrationEventMapper
{
    public IntegrationEvent? Map(object domainEvent)
    {
        return domainEvent switch
        {
            Domain.Todos.TodoItemCreatedDomainEvent evt => new TodoItemCreatedIntegrationEvent
            {
                EventType = nameof(TodoItemCreatedIntegrationEvent),
                TodoItemId = evt.TodoItemId,
                UserId = Guid.Empty,  // TODO: enrich from TodoItem entity if needed
                Description = string.Empty
            },
            Domain.Todos.TodoItemCompletedDomainEvent evt => new TodoItemCompletedIntegrationEvent
            {
                EventType = nameof(TodoItemCompletedIntegrationEvent),
                TodoItemId = evt.TodoItemId,
                UserId = Guid.Empty
            },
            Domain.Todos.TodoItemDeletedDomainEvent evt => new TodoItemDeletedIntegrationEvent
            {
                EventType = nameof(TodoItemDeletedIntegrationEvent),
                TodoItemId = evt.TodoItemId,
                UserId = Guid.Empty
            },
            _ => null
        };
    }
}
