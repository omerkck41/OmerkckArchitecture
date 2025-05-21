using Core.Localization.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Core.Localization.Configuration.Extensions;

/// <summary>
/// appsettings.json üzerinden yapılandırma için uzantı metotları
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// appsettings.json dosyasından lokalizasyon ayarlarını yükler
    /// </summary>
    /// <param name="services">Service koleksiyonu</param>
    /// <param name="configuration">IConfiguration nesnesi</param>
    /// <param name="sectionName">Ayarların bulunduğu section (varsayılan: "Localization")</param>
    /// <returns>Service koleksiyonu</returns>
    public static IServiceCollection AddJsonLocalizationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Localization")
    {
        // Konfigürasyon bölümünü al
        var section = configuration.GetSection(sectionName);

        // Eğer bölüm yoksa veya boşsa, varsayılan yapılandırma kullan
        if (!section.Exists())
        {
            return services.AddLocalization();
        }

        // Seçenekleri ayarla
        return services.AddLocalization(options =>
        {
            // Desteklenen kültürler
            var supportedCultures = section.GetSection("SupportedCultures").Get<string[]>();
            if (supportedCultures != null && supportedCultures.Length > 0)
            {
                options.SupportedCultures = supportedCultures
                    .Select(c => new CultureInfo(c))
                    .ToList();
            }

            // Varsayılan kültür
            var defaultCulture = section.GetValue<string>("DefaultCulture");
            if (!string.IsNullOrEmpty(defaultCulture))
            {
                options.DefaultCulture = new CultureInfo(defaultCulture);
            }

            // Yedek kültür
            var fallbackCulture = section.GetValue<string>("FallbackCulture");
            if (!string.IsNullOrEmpty(fallbackCulture))
            {
                options.FallbackCulture = new CultureInfo(fallbackCulture);
            }

            // Yedek kültürü kullan
            if (section.GetValue<bool?>("UseFallbackCulture") is bool useFallback)
            {
                options.UseFallbackCulture = useFallback;
            }

            // Kaynak yolları
            var resourcePaths = section.GetSection("ResourcePaths").Get<string[]>();
            if (resourcePaths != null && resourcePaths.Length > 0)
            {
                options.ResourcePaths = resourcePaths.ToList();
            }

            // Feature dizin deseni
            var featureDirectoryPattern = section.GetValue<string>("FeatureDirectoryPattern");
            if (!string.IsNullOrEmpty(featureDirectoryPattern))
            {
                options.FeatureDirectoryPattern = featureDirectoryPattern;
            }

            // Feature dosya deseni
            var featureFilePattern = section.GetValue<string>("FeatureFilePattern");
            if (!string.IsNullOrEmpty(featureFilePattern))
            {
                options.FeatureFilePattern = featureFilePattern;
            }

            // Varsayılan bölüm
            var defaultSection = section.GetValue<string>("DefaultSection");
            if (!string.IsNullOrEmpty(defaultSection))
            {
                options.DefaultSection = defaultSection;
            }

            // Kaynak bulunamadığında istisna fırlat
            if (section.GetValue<bool?>("ThrowOnMissingResource") is bool throwOnMissing)
            {
                options.ThrowOnMissingResource = throwOnMissing;
            }

            // Önbelleklemeyi etkinleştir
            if (section.GetValue<bool?>("EnableCaching") is bool enableCaching)
            {
                options.EnableCaching = enableCaching;
            }

            // Önbellek süresi
            var cacheExpirationMinutes = section.GetValue<int?>("CacheExpirationMinutes");
            if (cacheExpirationMinutes.HasValue && cacheExpirationMinutes.Value > 0)
            {
                options.CacheExpiration = TimeSpan.FromMinutes(cacheExpirationMinutes.Value);
            }

            // Dosya izlemeyi etkinleştir
            if (section.GetValue<bool?>("EnableResourceFileWatching") is bool enableFileWatching)
            {
                options.EnableResourceFileWatching = enableFileWatching;
            }

            // Otomatik keşfi etkinleştir
            if (section.GetValue<bool?>("EnableAutoDiscovery") is bool enableAutoDiscovery)
            {
                options.EnableAutoDiscovery = enableAutoDiscovery;
            }

            // Debug loglamayı etkinleştir
            if (section.GetValue<bool?>("EnableDebugLogging") is bool enableDebugLogging)
            {
                options.EnableDebugLogging = enableDebugLogging;
            }

            // Dağıtık önbelleklemeyi etkinleştir
            if (section.GetValue<bool?>("EnableDistributedCache") is bool enableDistributedCache)
            {
                options.EnableDistributedCache = enableDistributedCache;
            }

            // Desteklenen dosya uzantıları
            var resourceFileExtensions = section.GetSection("ResourceFileExtensions").Get<string[]>();
            if (resourceFileExtensions != null && resourceFileExtensions.Length > 0)
            {
                options.ResourceFileExtensions = resourceFileExtensions.ToList();
            }
        });
    }
}
