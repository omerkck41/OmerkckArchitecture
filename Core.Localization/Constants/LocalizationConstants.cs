namespace Core.Localization.Constants;

/// <summary>
/// Lokalizasyon modülü için sabit değerler
/// </summary>
public static class LocalizationConstants
{
    /// <summary>
    /// Yapılandırma bölümü adı
    /// </summary>
    public const string ConfigurationSectionName = "Localization";

    /// <summary>
    /// Çeviri dosyası adı şablonu
    /// </summary>
    public const string TranslationFileNameFormat = "locale.{0}.json";

    /// <summary>
    /// Önbellek anahtarı şablonu
    /// </summary>
    public const string CacheKeyFormat = "loc_{0}_{1}";

    /// <summary>
    /// Döviz kuru önbellek anahtarı
    /// </summary>
    public const string ExchangeRateCacheKey = "exchange_rates";
}