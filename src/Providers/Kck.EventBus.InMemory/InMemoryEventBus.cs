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
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IntegrationEvent
    {
        var eventType = typeof(TEvent);

        if (!_handlers.TryGetValue(eventType, out var handlerTypes))
        {
            await DispatchViaDi(@event, ct).ConfigureAwait(false);
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var handlers = handlerTypes.Keys
            .Select(t => (HandlerType: t, Handler: scope.ServiceProvider.GetService(t) as IEventHandler<TEvent>))
            .Where(x => x.Handler is not null);

        foreach (var (handlerType, handler) in handlers)
        {
            try
            {
                await handler!.HandleAsync(@event, ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.HandlerError(logger, ex, eventType.Name, handlerType.Name);
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

    private async Task DispatchViaDi<TEvent>(TEvent @event, CancellationToken ct)
        where TEvent : IntegrationEvent
    {
        using var scope = serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandleAsync(@event, ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.DispatchError(logger, ex, typeof(TEvent).Name);
            }
        }
    }
}
