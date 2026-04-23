using Kck.FeatureFlags.Abstractions;
using Kck.FeatureFlags.InMemory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckFeatureFlagsInMemoryServiceCollectionExtensions
{
    public static IServiceCollection AddKckFeatureFlagsInMemory(
        this IServiceCollection services,
        Action<InMemoryFeatureFlagOptions>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);
        else
            services.Configure<InMemoryFeatureFlagOptions>(_ => { });

        services.TryAddSingleton<IFeatureFlagService, InMemoryFeatureFlagService>();
        return services;
    }
}
