using System.Collections.Concurrent;
using System.Reflection;
using Kck.Core.Abstractions.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Kck.Persistence.EntityFramework.Interceptors;

/// <summary>
/// Dispatches each domain event to the <see cref="IDomainEventHandler{TEvent}"/> services registered for its exact
/// runtime type (ADR-0030). Registered by <see cref="KckPersistenceBuilder.AddDomainEventHandlers"/>.
/// </summary>
internal sealed class ServiceProviderDomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, Func<IServiceProvider, IDomainEvent, CancellationToken, Task>> Invokers = new();

    private static readonly MethodInfo DispatchOneMethod = typeof(ServiceProviderDomainEventDispatcher)
        .GetMethod(nameof(DispatchOneAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);

        foreach (var domainEvent in events)
        {
            var invoke = Invokers.GetOrAdd(domainEvent.GetType(), CreateInvoker);
            await invoke(serviceProvider, domainEvent, cancellationToken);
        }
    }

    private static Func<IServiceProvider, IDomainEvent, CancellationToken, Task> CreateInvoker(Type eventType) =>
        DispatchOneMethod.MakeGenericMethod(eventType)
            .CreateDelegate<Func<IServiceProvider, IDomainEvent, CancellationToken, Task>>();

    private static async Task DispatchOneAsync<TEvent>(
        IServiceProvider services, IDomainEvent domainEvent, CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        foreach (var handler in services.GetServices<IDomainEventHandler<TEvent>>())
            await handler.HandleAsync((TEvent)domainEvent, cancellationToken);
    }
}
