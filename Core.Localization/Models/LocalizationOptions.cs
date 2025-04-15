namespace Core.Localization.Models;

/// <summary>
/// Lokalizasyon modülü için yapılandırma seçenekleri
/// </summary>
public class LocalizationOptions
{
    /// <summary>
    /// Varsayılan kültür kodu (örn. "tr-TR")
    /// </summary>
    public string DefaultCulture { get; set; } = "tr-TR";

    /// <summary>
    /// Desteklenen kültür kodları
    /// </summary>
    public List<string> SupportedCultures { get; set; } = ["tr-TR", "en-US"];

    /// <summary>
    /// Çeviri bulunamazsa anahtar gösterilsin mi?
    /// </summary>
    public bool ReturnKeyIfNotFound { get; set; } = true;

    /// <summary>
    /// Çeviri kaynakları ile ilgili yapılandırma
    /// </summary>
    public LocalizationSourceOptions Sources { get; set; } = new();

    /// <summary>
    /// Para birimi ayarları
    /// </summary>
    public CurrencyOptions Currency { get; set; } = new();

    /// <summary>
    /// Tarih formatı ayarları
    /// </summary>
    public DateTimeOptions DateTime { get; set; } = new();
}