using Core.Localization.Abstractions;
using Core.Localization.Configuration;
using Core.Localization.Providers;
using Core.Localization.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core.Localization.Extensions;

/// <summary>
/// Extension methods for registering localization services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Core.Localization services to the service collection with async support
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional action to configure options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddLocalization(
        this IServiceCollection services,
        Action<LocalizationOptions>? configureOptions = null)
    {
        // Register default options
        services.AddOptions<LocalizationOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection("Localization").Bind(options);
                configureOptions?.Invoke(options);
            });

        // Register core services
        services.AddMemoryCache();
        services.AddLogging();

        // Register providers
        services.AddSingleton<IResourceProvider, YamlResourceProvider>();
        services.TryAddSingleton<IResourceProvider, JsonResourceProvider>();

        // Register main services
        services.TryAddSingleton<ILocalizationService, LocalizationService>();
        services.TryAddSingleton<IFormatterService, FormatterService>();

        return services;
    }

    /// <summary>
    /// Adds Core.Localization services with feature-based localization support
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional action to configure options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFeatureBasedLocalization(
        this IServiceCollection services,
        Action<LocalizationOptions>? configureOptions = null)
    {
        return services.AddLocalization(options =>
        {
            // Enable feature-based localization by default
            options.EnableAutoDiscovery = true;
            options.UseFileSystemWatcher = true;
            options.ResourcePaths = new List<string> { "Features" };
            options.FeatureDirectoryPattern = "**/Resources/Locales";
            options.FeatureFilePattern = "{section}.{culture}.{extension}";

            // Apply custom configuration if provided
            configureOptions?.Invoke(options);
        });
    }

    /// <summary>
    /// Adds a custom resource provider
    /// </summary>
    /// <typeparam name="TProvider">The resource provider type</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddResourceProvider<TProvider>(
        this IServiceCollection services)
        where TProvider : class, IResourceProvider
    {
        services.AddSingleton<IResourceProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Adds only JSON resource provider
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddJsonResourceProvider(this IServiceCollection services)
    {
        services.RemoveAll<IResourceProvider>();
        services.AddSingleton<IResourceProvider, JsonResourceProvider>();
        return services;
    }

    /// <summary>
    /// Adds only YAML resource provider
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddYamlResourceProvider(this IServiceCollection services)
    {
        services.RemoveAll<IResourceProvider>();
        services.AddSingleton<IResourceProvider, YamlResourceProvider>();
        return services;
    }

    /// <summary>
    /// Configures localization options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection ConfigureLocalization(
        this IServiceCollection services,
        Action<LocalizationOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// Adds in-memory resource provider for testing or simple scenarios
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="resources">Dictionary of resources</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInMemoryResourceProvider(
        this IServiceCollection services,
        IDictionary<string, IDictionary<string, string>> resources)
    {
        services.AddSingleton<IResourceProvider>(
            provider => new InMemoryResourceProvider(resources));
        return services;
    }

    /// <summary>
    /// Configures localization with feature paths
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="featurePaths">Feature path patterns</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFeaturePaths(
        this IServiceCollection services,
        params string[] featurePaths)
    {
        services.Configure<LocalizationOptions>(options =>
        {
            options.ResourcePaths = featurePaths.ToList();
        });

        return services;
    }

    /// <summary>
    /// Configures localization for distributed (cloud) scenarios
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDistributedLocalization(
        this IServiceCollection services)
    {
        services.Configure<LocalizationOptions>(options =>
        {
            options.EnableDistributedCache = true;
            options.EnableResourceFileWatching = false;
        });

        // Add distributed cache if not already added
        services.AddDistributedMemoryCache();

        return services;
    }
}
