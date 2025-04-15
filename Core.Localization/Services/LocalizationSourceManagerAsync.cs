using Core.Localization.Abstract;
using System.Globalization;

namespace Core.Localization.Services;

/// <summary>
/// Manages localization sources and provides methods to load translations asynchronously.
/// </summary>
public class LocalizationSourceManagerAsync
{
    private readonly IEnumerable<ILocalizationSourceAsync> _sources;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationSourceManagerAsync"/> class.
    /// </summary>
    /// <param name="sources">The collection of localization sources.</param>
    public LocalizationSourceManagerAsync(IEnumerable<ILocalizationSourceAsync> sources)
    {
        _sources = sources;
    }

    /// <summary>
    /// Asynchronously loads and merges translations from all sources for the specified culture.
    /// </summary>
    /// <param name="culture">The culture for which to load translations.</param>
    /// <returns>A dictionary containing the merged translations.</returns>
    public async Task<IDictionary<string, string>> LoadAllTranslationsAsync(CultureInfo culture)
    {
        // Tüm kaynak sağlayıcıları paralel olarak çağırın.
        var tasks = _sources.Select(src => src.GetTranslationsAsync(culture));
        var results = await Task.WhenAll(tasks);

        // Gelen sözlükleri birleştirirken, çakışmaları yönetmek için öncelik sırası belirleyin.
        var mergedTranslations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var dict in results)
        {
            foreach (var kvp in dict)
            {
                // Eğer aynı anahtar farklı kaynaklarda varsa, öncelik sırası uygulanabilir.
                if (!mergedTranslations.ContainsKey(kvp.Key))
                {
                    mergedTranslations[kvp.Key] = kvp.Value;
                }
            }
        }

        return mergedTranslations;
    }
}
