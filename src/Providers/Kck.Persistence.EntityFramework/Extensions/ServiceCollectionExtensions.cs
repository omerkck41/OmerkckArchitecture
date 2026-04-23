using Microsoft.Extensions.DependencyInjection;

namespace Kck.Persistence.EntityFramework.Extensions;

/// <summary>
/// Extension methods for registering Kck persistence services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Kck persistence services using the builder pattern.
    /// </summary>
    public static IServiceCollection AddKckPersistence(
        this IServiceCollection services,
        Action<KckPersistenceBuilder> configure)
    {
        var builder = new KckPersistenceBuilder(services);
        configure(builder);
        return services;
    }
}
