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
    public static IServiceCollection AddCoreLocalization(
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
        services.AddSingleton<IResourceProvider, JsonResourceProvider>();

        // Register main services
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IFormatterService, FormatterService>();

        return services;
    }

    /// <summary>
    /// Adds Core.Localization services with feature-based localization support
    /// </summary>
    public static IServiceCollection AddFeatureBasedLocalization(
        this IServiceCollection services,
        Action<LocalizationOptions>? configureOptions = null)
    {
        return services.AddCoreLocalization(options =>
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
    public static IServiceCollection AddJsonResourceProvider(this IServiceCollection services)
    {
        services.RemoveAll<IResourceProvider>();
        services.AddSingleton<IResourceProvider, JsonResourceProvider>();
        return services;
    }

    /// <summary>
    /// Adds only YAML resource provider
    /// </summary>
    public static IServiceCollection AddYamlResourceProvider(this IServiceCollection services)
    {
        services.RemoveAll<IResourceProvider>();
        services.AddSingleton<IResourceProvider, YamlResourceProvider>();
        return services;
    }

    /// <summary>
    /// Configures localization options
    /// </summary>
    public static IServiceCollection ConfigureLocalization(
        this IServiceCollection services,
        Action<LocalizationOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// Adds in-memory resource provider for testing
    /// </summary>
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
    /// Configures localization to use specific feature directory pattern
    /// </summary>
    public static IServiceCollection UseFeatureDirectoryPattern(
        this IServiceCollection services,
        string pattern)
    {
        services.Configure<LocalizationOptions>(options =>
        {
            options.FeatureDirectoryPattern = pattern;
        });

        return services;
    }

    /// <summary>
    /// Configures localization to use specific feature file pattern
    /// </summary>
    public static IServiceCollection UseFeatureFilePattern(
        this IServiceCollection services,
        string pattern)
    {
        services.Configure<LocalizationOptions>(options =>
        {
            options.FeatureFilePattern = pattern;
        });

        return services;
    }
}
