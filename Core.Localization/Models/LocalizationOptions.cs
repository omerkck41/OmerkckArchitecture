namespace Core.Localization.Models;

/// <summary>
/// Lokalizasyon modülü için yapılandırma seçenekleri
/// </summary>
public class LocalizationOptions
{
    /// <summary>Varsayılan kültür (örn. "tr-TR")</summary>
    public string DefaultCulture { get; set; } = "tr-TR";

    /// <summary>Desteklenen kültürler listesi</summary>
    public List<string> SupportedCultures { get; set; }
        = new List<string> { "tr-TR", "en-US" };

    /// <summary>Çeviri bulunamazsa key döndürsün mü?</summary>
    public bool ReturnKeyIfNotFound { get; set; } = true;

    /// <summary>Önbellek (cache) Time‑To‑Live (saniye)</summary>
    public int CacheTtlSeconds { get; set; } = 3600;

    /// <summary>Fallback (yedek) kültür</summary>
    public string FallbackCulture { get; set; } = "en-US";

    /// <summary>Çeviri önbelleğini kullanmak istiyor muyuz?</summary>
    public bool EnableCaching { get; set; } = true;
}