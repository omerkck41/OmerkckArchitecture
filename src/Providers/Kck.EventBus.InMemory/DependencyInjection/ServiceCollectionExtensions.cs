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
}
