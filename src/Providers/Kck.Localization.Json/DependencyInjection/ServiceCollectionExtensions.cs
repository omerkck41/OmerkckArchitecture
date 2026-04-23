using Kck.Localization.Abstractions;
using Kck.Localization.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class KckLocalizationJsonServiceCollectionExtensions
{
    /// <summary>
    /// Adds the JSON resource provider for localization.
    /// Call AddKckLocalization() first to configure options and core services.
    /// </summary>
    public static IServiceCollection AddKckLocalizationJson(
        this IServiceCollection services,
        int priority = 100)
    {
        services.AddSingleton<IResourceProvider>(sp =>
            new JsonResourceProvider(
                sp.GetRequiredService<IOptionsMonitor<LocalizationOptions>>(),
                sp.GetRequiredService<ILogger<JsonResourceProvider>>(),
                priority));

        return services;
    }
}
