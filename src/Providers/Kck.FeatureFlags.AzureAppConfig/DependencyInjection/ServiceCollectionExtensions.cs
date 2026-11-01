using Azure.Data.AppConfiguration;
using Kck.FeatureFlags.Abstractions;
using Kck.FeatureFlags.AzureAppConfig;
using Kck.FeatureFlags.AzureAppConfig.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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
    /// When a <see cref="ConfigurationClient"/> is registered in the container, the service uses it instead of
    /// building one from the options: register your own client for custom credentials or a custom endpoint. With a
    /// registered client, <c>ConnectionString</c> and <c>Endpoint</c> may both stay empty.
    /// </summary>
    public static IServiceCollection AddKckFeatureFlagsAzureAppConfig(
        this IServiceCollection services,
        Action<AzureAppConfigOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<IValidateOptions<AzureAppConfigOptions>, AzureAppConfigFeatureFlagOptionsValidator>();
        services.AddOptions<AzureAppConfigOptions>().ValidateOnStart();
        services.TryAddSingleton<IFeatureFlagService>(sp =>
        {
            var options = sp.GetRequiredService<IOptionsMonitor<AzureAppConfigOptions>>();
            var logger = sp.GetRequiredService<ILogger<AzureAppConfigFeatureFlagService>>();
            return sp.GetService<ConfigurationClient>() is { } client
                ? new AzureAppConfigFeatureFlagService(options, logger, client)
                : new AzureAppConfigFeatureFlagService(options, logger);
        });
        return services;
    }
}
