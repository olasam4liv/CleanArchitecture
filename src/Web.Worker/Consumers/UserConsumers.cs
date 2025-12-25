namespace Web.Worker.Consumers;

/// <summary>
/// Consumer that receives UserRegisteredIntegrationEvent and logs it.
/// This demonstrates end-to-end event handling in a separate worker process.
/// </summary>
public sealed class UserRegisteredConsumer : IConsumer<UserRegisteredIntegrationEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        UserRegisteredIntegrationEvent @event = context.Message;

        _logger.LogInformation(
            "Received UserRegisteredIntegrationEvent: UserId={UserId}, Email={Email}, Name={FirstName} {LastName}",
            @event.UserId,
            @event.Email,
            @event.FirstName,
            @event.LastName);

        // TODO: Perform side effects here (e.g., send welcome email, create user profile, init analytics, etc.)

        return Task.CompletedTask;
    }
}
