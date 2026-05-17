using Kck.FeatureFlags.Abstractions;
using Kck.FeatureFlags.AzureAppConfig;
using Kck.FeatureFlags.AzureAppConfig.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering Azure App Configuration-backed feature flags.
/// </summary>
public static class KckFeatureFlagsAzureAppConfigServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="AzureAppConfigFeatureFlagService"/> as the <see cref="IFeatureFlagService"/>.
    /// Provide either <c>ConnectionString</c> or <c>Endpoint</c> (Managed Identity) in the options.
    /// </summary>
    public static IServiceCollection AddKckFeatureFlagsAzureAppConfig(
        this IServiceCollection services,
        Action<AzureAppConfigOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<AzureAppConfigOptions>, AzureAppConfigFeatureFlagOptionsValidator>();
        services.AddOptions<AzureAppConfigOptions>().ValidateOnStart();
        services.TryAddSingleton<IFeatureFlagService, AzureAppConfigFeatureFlagService>();
        return services;
    }
}
