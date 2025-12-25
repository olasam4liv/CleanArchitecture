using System.Reflection;
using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Messaging;

internal sealed class MassTransitEventBus(
    IPublishEndpoint publishEndpoint,
    ILogger<MassTransitEventBus> logger) : IEventBus
{
    public async Task PublishAsync(string type, string content, CancellationToken cancellationToken = default)
    {
        Type? messageType = ResolveType(type);
        if (messageType == null)
        {
            logger.LogWarning("Unknown message type '{Type}', skipping publish.", type);
            return;
        }

        object? message = JsonSerializer.Deserialize(content, messageType);
        if (message == null)
        {
            logger.LogWarning("Failed to deserialize content for type '{Type}', skipping publish.", type);
            return;
        }

        await publishEndpoint.Publish(message, messageType, cancellationToken);
        logger.LogInformation("Published {Type} via MassTransit", type);
    }

    private static Type? ResolveType(string typeName)
    {
        // Try full-name across loaded assemblies; integration event types should be public.
        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? t = asm.GetType(typeName, throwOnError: false, ignoreCase: false);
            if (t != null)
            {
                return t;
            }
        }
        return null;
    }
}
