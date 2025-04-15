using Core.Localization.Abstract;

namespace Core.Localization.Extensions;

/// <summary>
/// String sınıfı için uzantı metotları
/// </summary>
public static class StringExtensions
{
    private static ILocalizationServiceAsync? _localizationService;

    /// <summary>
    /// Lokalizasyon servisini ayarlar (sistem tarafından kullanılır)
    /// </summary>
    internal static void SetLocalizationService(ILocalizationServiceAsync localizationService)
    {
        _localizationService = localizationService;
    }

    /// <summary>
    /// Metni yerelleştirir
    /// </summary>
    /// <param name="text">Çevrilecek metin anahtarı</param>
    /// <returns>Çevrilmiş metin</returns>
    public static async Task<string> L(this string text)
    {
        if (_localizationService == null)
        {
            throw new InvalidOperationException("Localization service is not set. Ensure localization is configured properly.");
        }

        return await _localizationService.GetStringAsync(text);
    }

    /// <summary>
    /// Metni parametrelerle birlikte yerelleştirir
    /// </summary>
    /// <param name="text">Çevrilecek metin anahtarı</param>
    /// <param name="args">Format parametreleri</param>
    /// <returns>Çevrilmiş ve formatlanmış metin</returns>
    public static async Task<string> L(this string text, params object[] args)
    {
        if (_localizationService == null)
        {
            throw new InvalidOperationException("Localization service is not set. Ensure localization is configured properly.");
        }

        return await _localizationService.GetStringAsync(text, args);
    }
}
