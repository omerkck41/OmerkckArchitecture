using System.Collections.Concurrent;
using Kck.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kck.EventBus.InMemory;

/// <summary>
/// In-memory implementation of <see cref="IEventBus"/> for local event dispatching.
/// </summary>
public sealed class InMemoryEventBus(
    IServiceProvider serviceProvider,
    ILogger<InMemoryEventBus> logger) : IEventBus
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, byte>> _handlers = new();

    /// <inheritdoc />
    public async Task PublishAsync<TEvent>(TEvent eventData, CancellationToken ct = default)
        where TEvent : IntegrationEvent
    {
        using var scope = serviceProvider.CreateScope();
        foreach (var handler in ResolveHandlers<TEvent>(scope.ServiceProvider))
        {
            try
            {
                await handler.HandleAsync(eventData, ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.HandlerError(logger, ex, typeof(TEvent).Name, handler.GetType().Name);
            }
        }
    }

    /// <inheritdoc />
    public void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IEventHandler<TEvent>
    {
        var handlerTypes = _handlers.GetOrAdd(typeof(TEvent), _ => new ConcurrentDictionary<Type, byte>());
        handlerTypes.TryAdd(typeof(THandler), 0);
    }

    // Every registered IEventHandler<TEvent>, plus each subscribed handler class that is not among them (resolved by its
    // class). Before 4.7 a subscription replaced the registered handlers and looked only for the class, so handlers
    // registered through RegisterHandlersFromAssembly were silently skipped.
    private List<IEventHandler<TEvent>> ResolveHandlers<TEvent>(IServiceProvider services)
        where TEvent : IntegrationEvent
    {
        var handlers = services.GetServices<IEventHandler<TEvent>>().ToList();
        if (!_handlers.TryGetValue(typeof(TEvent), out var subscribed))
            return handlers;

        foreach (var handlerType in subscribed.Keys.Where(t => handlers.TrueForAll(h => h.GetType() != t)))
        {
            if (services.GetService(handlerType) is IEventHandler<TEvent> handler)
                handlers.Add(handler);
            else
                Log.SubscribedHandlerNotRegistered(logger, handlerType.Name, typeof(TEvent).Name);
        }

        return handlers;
    }
}
