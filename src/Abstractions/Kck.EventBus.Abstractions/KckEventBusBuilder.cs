using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Kck.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Builder for configuring event bus services.
/// </summary>
public sealed class KckEventBusBuilder(IServiceCollection services)
{
    /// <summary>
    /// Gets the underlying <see cref="IServiceCollection"/> used to register event bus services.
    /// </summary>
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Scans the specified assemblies and registers all <see cref="IEventHandler{TEvent}"/> implementations.
    /// </summary>
    [RequiresUnreferencedCode("Scans assemblies using reflection to discover IEventHandler implementations. Types may be trimmed.")]
    [RequiresDynamicCode("Registers discovered handler types via dynamic DI registration.")]
    public KckEventBusBuilder RegisterHandlersFromAssembly(params Assembly[] assemblies)
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

                // TryAddEnumerable: scanning an assembly twice must not run a handler twice.
                foreach (var @interface in interfaces)
                    Services.TryAddEnumerable(ServiceDescriptor.Transient(@interface, type));

                // The class too: Subscribe<TEvent, THandler>() resolves the handler by its class, without building
                // every other handler of the event.
                if (!type.ContainsGenericParameters)
                    Services.TryAddTransient(type);
            }
        }

        return this;
    }
}
