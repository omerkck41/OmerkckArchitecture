using Kck.Localization.Abstractions;
using Kck.Localization.Yaml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckLocalizationYamlServiceCollectionExtensions
{
    /// <summary>
    /// Adds the YAML resource provider for localization.
    /// Call AddKckLocalization() first to configure options and core services.
    /// </summary>
    public static IServiceCollection AddKckLocalizationYaml(
        this IServiceCollection services,
        int priority = 100)
    {
        services.AddSingleton<IResourceProvider>(sp =>
            new YamlResourceProvider(
                sp.GetRequiredService<IOptionsMonitor<LocalizationOptions>>(),
                sp.GetRequiredService<ILogger<YamlResourceProvider>>(),
                priority));

        return services;
    }
}
