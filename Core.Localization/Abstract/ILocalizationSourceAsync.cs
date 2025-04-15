using System.Globalization;

namespace Core.Localization.Abstract;

/// <summary>
/// Lokalizasyon kaynak sağlayıcısı için temel sözleşme
/// </summary>
public interface ILocalizationSourceAsync
{
    /// <summary>
    /// Kaynağın adı.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Belirtilen kültür için çeviri anahtar-değer çiftlerini getirir.
    /// </summary>
    Task<IDictionary<string, string>> GetTranslationsAsync(CultureInfo culture);

    /// <summary>
    /// Kaynağı başlatır.
    /// </summary>
    Task<bool> InitializeAsync();

    /// <summary>
    /// Kaynak güncellemelerini (örneğin, veritabanı veya API'den) yeniler.
    /// </summary>
    Task RefreshAsync();

    /// <summary>
    /// Desteklenen kültürlerin listesini döndürür.
    /// </summary>
    IEnumerable<CultureInfo> GetSupportedCultures();
}
