using Kck.Localization;
using Kck.Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckLocalizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core localization services (LocalizationService, DefaultPluralizer, FormatterService).
    /// Resource providers must be registered separately (e.g., AddKckLocalizationJson).
    /// </summary>
    public static IServiceCollection AddKckLocalization(
        this IServiceCollection services,
        Action<LocalizationOptions>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);
        else
            services.Configure<LocalizationOptions>(_ => { });

        services.TryAddSingleton<IPluralizer, DefaultPluralizer>();
        services.TryAddSingleton<IFormatterService, FormatterService>();
        services.TryAddSingleton<ILocalizationService, LocalizationService>();

        return services;
    }

    /// <summary>
    /// Adds the InMemoryResourceProvider. Useful for testing or development scenarios.
    /// </summary>
    public static IServiceCollection AddKckLocalizationInMemory(
        this IServiceCollection services,
        IDictionary<string, Dictionary<string, string>> resources)
    {
        services.AddSingleton<IResourceProvider>(new InMemoryResourceProvider(resources));
        return services;
    }

    /// <summary>
    /// Adds the InMemoryResourceProvider with a configuration callback.
    /// </summary>
    public static IServiceCollection AddKckLocalizationInMemory(
        this IServiceCollection services,
        Action<InMemoryResourceProvider> configure)
    {
        var provider = new InMemoryResourceProvider();
        configure(provider);
        services.AddSingleton<IResourceProvider>(provider);
        return services;
    }
}
