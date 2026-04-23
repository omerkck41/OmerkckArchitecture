using System.Reflection;
using Kck.EventBus.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Builder for configuring event bus services.
/// </summary>
public sealed class KckEventBusBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;

    /// <summary>
    /// Scans the specified assemblies and registers all <see cref="IEventHandler{TEvent}"/> implementations.
    /// </summary>
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

                foreach (var @interface in interfaces)
                    Services.AddTransient(@interface, type);
            }
        }

        return this;
    }
}
