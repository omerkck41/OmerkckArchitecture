using System.Reflection;
using Kck.EventBus.Abstractions;
using Kck.EventBus.InMemory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering in-memory event bus services.
/// </summary>
public static class KckEventBusInMemoryServiceCollectionExtensions
{
    /// <summary>
    /// Configures the event bus to use the in-memory provider.
    /// </summary>
    public static KckEventBusBuilder UseInMemory(this KckEventBusBuilder builder)
    {
        builder.Services.TryAddSingleton<IEventBus, InMemoryEventBus>();
        return builder;
    }

#pragma warning disable CS0618 // Obsolete members used internally for backward compat
    /// <summary>
    /// Adds the in-memory event bus to the service collection.
    /// </summary>
    [Obsolete("Use AddKckEventBus(b => b.UseInMemory()) instead.")]
    public static IServiceCollection AddKckEventBusInMemory(this IServiceCollection services)
    {
        services.TryAddSingleton<IEventBus, InMemoryEventBus>();
        return services;
    }

    /// <summary>
    /// Adds the in-memory event bus and auto-registers all event handlers from the specified assemblies.
    /// </summary>
    [Obsolete("Use AddKckEventBus(b => { b.UseInMemory(); b.RegisterHandlersFromAssembly(...); }) instead.")]
    public static IServiceCollection AddKckEventBusInMemory(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.TryAddSingleton<IEventBus, InMemoryEventBus>();
        RegisterHandlers(services, assemblies);
        return services;
    }
#pragma warning restore CS0618

    private static void RegisterHandlers(IServiceCollection services, Assembly[] assemblies)
    {
        var handlerInterface = typeof(IEventHandler<>);
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false })
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface));

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);

                foreach (var @interface in interfaces)
                    services.AddTransient(@interface, type);
            }
        }
    }
}
