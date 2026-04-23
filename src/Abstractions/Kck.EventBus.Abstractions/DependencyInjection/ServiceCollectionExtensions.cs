namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering event bus services via builder pattern.
/// </summary>
public static class KckEventBusServiceCollectionExtensions
{
    /// <summary>
    /// Adds event bus services using the builder pattern.
    /// </summary>
    public static IServiceCollection AddKckEventBus(
        this IServiceCollection services,
        Action<KckEventBusBuilder> configure)
    {
        var builder = new KckEventBusBuilder(services);
        configure(builder);
        return services;
    }
}
