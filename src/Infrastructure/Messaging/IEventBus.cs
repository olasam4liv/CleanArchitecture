namespace Infrastructure.Messaging;

public interface IEventBus
{
    Task PublishAsync(string type, string content, CancellationToken cancellationToken = default);
}
