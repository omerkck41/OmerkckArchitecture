using System.Collections.Concurrent;
using System.Reflection;

namespace Kck.EventBus.Abstractions;

/// <summary>
/// Helper for invoking <see cref="IEventHandler{TEvent}"/> implementations at runtime
/// when the generic type is not known at compile time.
/// </summary>
public static class EventHandlerInvoker
{
    private static readonly string HandleAsyncMethodName =
        nameof(IEventHandler<IntegrationEvent>.HandleAsync);

    private static readonly ConcurrentDictionary<Type, MethodInfo> MethodCache = new();

    /// <summary>
    /// Invokes HandleAsync on a handler instance using the specified event type.
    /// </summary>
    public static async Task InvokeAsync(
        object handler,
        object @event,
        Type eventType,
        CancellationToken ct = default)
    {
        var method = MethodCache.GetOrAdd(eventType, static et =>
        {
            var handlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(et);
            return handlerInterfaceType.GetMethod(HandleAsyncMethodName)
                ?? throw new InvalidOperationException(
                    $"Method '{HandleAsyncMethodName}' not found on {handlerInterfaceType.FullName}");
        });

        var task = (Task)method.Invoke(handler, [@event, ct])!;
        await task.ConfigureAwait(false);
    }
}
