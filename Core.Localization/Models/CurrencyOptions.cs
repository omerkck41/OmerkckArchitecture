namespace Core.Localization.Models;

/// <summary>
/// Para birimi ayarları
/// </summary>
public class CurrencyOptions
{
    /// <summary>
    /// Varsayılan para birimi
    /// </summary>
    public string DefaultCurrency { get; set; } = "TRY";

    /// <summary>
    /// Döviz kuru API endpoint'i
    /// </summary>
    public string? ExchangeRateApiUrl { get; set; }

    /// <summary>
    /// Döviz kuru API anahtarı
    /// </summary>
    public string? ExchangeRateApiKey { get; set; }

    /// <summary>
    /// Döviz kuru güncelleme aralığı (dakika)
    /// </summary>
    public int UpdateInterval { get; set; } = 60;

    /// <summary>
    /// Döviz kuru önbelleği etkin mi?
    /// </summary>
    public bool EnableCaching { get; set; } = true;
}
