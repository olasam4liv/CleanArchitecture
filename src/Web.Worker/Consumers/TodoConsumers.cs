namespace Web.Worker.Consumers;

/// <summary>
/// Consumer that receives TodoItemCreatedIntegrationEvent and logs it.
/// This demonstrates end-to-end event handling in a separate worker process.
/// </summary>
public sealed class TodoItemCreatedConsumer : IConsumer<TodoItemCreatedIntegrationEvent>
{
    private readonly ILogger<TodoItemCreatedConsumer> _logger;

    public TodoItemCreatedConsumer(ILogger<TodoItemCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<TodoItemCreatedIntegrationEvent> context)
    {
        TodoItemCreatedIntegrationEvent @event = context.Message;

        _logger.LogInformation(
            "Received TodoItemCreatedIntegrationEvent: TodoItemId={TodoItemId}, UserId={UserId}, Description={Description}",
            @event.TodoItemId,
            @event.UserId,
            @event.Description);

        // TODO: Perform side effects here (e.g., update search index, send notification, etc.)

        return Task.CompletedTask;
    }
}

/// <summary>
/// Consumer that receives TodoItemCompletedIntegrationEvent and logs it.
/// </summary>
public sealed class TodoItemCompletedConsumer : IConsumer<TodoItemCompletedIntegrationEvent>
{
    private readonly ILogger<TodoItemCompletedConsumer> _logger;

    public TodoItemCompletedConsumer(ILogger<TodoItemCompletedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<TodoItemCompletedIntegrationEvent> context)
    {
        TodoItemCompletedIntegrationEvent @event = context.Message;

        _logger.LogInformation(
            "Received TodoItemCompletedIntegrationEvent: TodoItemId={TodoItemId}, UserId={UserId}",
            @event.TodoItemId,
            @event.UserId);

        // TODO: Perform side effects (e.g., update analytics, trigger notification, etc.)

        return Task.CompletedTask;
    }
}

/// <summary>
/// Consumer that receives TodoItemDeletedIntegrationEvent and logs it.
/// </summary>
public sealed class TodoItemDeletedConsumer : IConsumer<TodoItemDeletedIntegrationEvent>
{
    private readonly ILogger<TodoItemDeletedConsumer> _logger;

    public TodoItemDeletedConsumer(ILogger<TodoItemDeletedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<TodoItemDeletedIntegrationEvent> context)
    {
        TodoItemDeletedIntegrationEvent @event = context.Message;

        _logger.LogInformation(
            "Received TodoItemDeletedIntegrationEvent: TodoItemId={TodoItemId}, UserId={UserId}",
            @event.TodoItemId,
            @event.UserId);

        // TODO: Perform side effects (e.g., cleanup related data, update search index, etc.)

        return Task.CompletedTask;
    }
}
