using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kck.EventBus.Abstractions;

/// <summary>
/// Shared logic for deserializing and dispatching integration events to their handlers.
/// Used by both RabbitMQ and Azure Service Bus implementations.
/// </summary>
public sealed class EventProcessor(
    IServiceProvider serviceProvider,
    ILogger logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Deserializes the event body and invokes all registered handlers.
    /// </summary>
    public async Task ProcessAsync(
        string eventName,
        byte[] body,
        ConcurrentDictionary<string, Type> eventTypes,
        ConcurrentDictionary<string, ConcurrentBag<Type>> handlers)
    {
        if (!eventTypes.TryGetValue(eventName, out var eventType))
            return;

        if (!handlers.TryGetValue(eventName, out var handlerTypes))
            return;

        var @event = JsonSerializer.Deserialize(body, eventType, JsonOptions);
        if (@event is null)
            return;

        if (@event is not IntegrationEvent)
        {
            Log.NotAnIntegrationEvent(logger, eventName);
            return;
        }

        using var scope = serviceProvider.CreateScope();
        foreach (var handlerType in handlerTypes)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);
            if (handler is null)
            {
                Log.NoHandlerRegistered(logger, handlerType.Name, eventName);
                continue;
            }

            await EventHandlerInvoker.InvokeAsync(handler, @event, eventType).ConfigureAwait(false);
        }
    }
}
