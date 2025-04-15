using Core.Localization.Abstract;
using Core.Localization.Cache;
using Core.Localization.Constants;
using Core.Localization.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Core.Localization.Services;

/// <summary>
/// Manages localization sources and provides methods to load translations asynchronously.
/// </summary>
public class LocalizationSourceManagerAsync
{
    private readonly IEnumerable<ILocalizationSourceAsync> _sources;
    private readonly IDistributedCacheManagerAsync _cacheManager;
    private readonly LocalizationOptions _options;
    private readonly ILogger<LocalizationSourceManagerAsync> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationSourceManagerAsync"/> class.
    /// </summary>
    /// <param name="sources">The collection of localization sources.</param>
    public LocalizationSourceManagerAsync(
         IEnumerable<ILocalizationSourceAsync> sources,
         IDistributedCacheManagerAsync cacheManager,
         IOptions<LocalizationOptions> options,
         ILogger<LocalizationSourceManagerAsync> logger)
    {
        // Kaynakları öncelik sırasına göre sırala (Örnek: Veritabanı > JSON > Resource)
        // Bu sıralama ILocalizationSourceAsync implementasyonlarının DI kaydına göre veya
        // kaynaklara eklenecek bir 'Priority' özelliğine göre yapılabilir.
        _sources = sources;
        _cacheManager = cacheManager;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously loads and merges translations from all sources for the specified culture.
    /// </summary>
    /// <param name="culture">The culture for which to load translations.</param>
    /// <returns>A dictionary containing the merged translations.</returns>
    public async Task<IDictionary<string, string>> LoadTranslationsForCultureAsync(CultureInfo culture)
    {
        string cacheKey = string.Format(LocalizationConstants.CacheKeyFormat, "Translations", culture.Name);

        // 1. Önbelleği kontrol et
        var cachedTranslations = await _cacheManager.GetAsync<Dictionary<string, string>>(cacheKey);
        if (cachedTranslations != null)
        {
            _logger.LogDebug("Translations for culture {Culture} loaded from cache.", culture.Name);
            return cachedTranslations;
        }

        _logger.LogDebug("Translations for culture {Culture} not found in cache. Loading from sources.", culture.Name);

        // 2. Kaynaklardan yükle (Önbellekte yoksa)
        var mergedTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        // Kaynakları öncelik sırasına göre işle (varsayılan DI sırası)
        foreach (var source in _sources)
        {
            try
            {
                var sourceTranslations = await source.GetTranslationsAsync(culture);
                foreach (var kvp in sourceTranslations)
                {
                    // Daha önce eklenmemişse ekle (ilk bulunan kaynak öncelikli)
                    if (!mergedTranslations.ContainsKey(kvp.Key))
                    {
                        mergedTranslations.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading translations from source {SourceName} for culture {Culture}", source.Name, culture.Name);
                // Hata durumunda diğer kaynaklarla devam et
            }
        }

        // 3. Önbelleğe ekle
        if (mergedTranslations.Any() && _options.Sources.EnableCaching)
        {
            var cacheDuration = TimeSpan.FromMinutes(_options.Sources.CacheDurationMinutes);
            await _cacheManager.SetAsync(cacheKey, mergedTranslations, cacheDuration);
            _logger.LogDebug("Translations for culture {Culture} added to cache with duration {CacheDuration}.", culture.Name, cacheDuration);
        }

        return mergedTranslations;
    }
}
