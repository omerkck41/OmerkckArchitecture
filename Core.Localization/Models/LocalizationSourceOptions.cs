namespace Core.Localization.Models;

/// <summary>
/// Lokalizasyon kaynaklarına ait yapılandırma seçenekleri
/// </summary>
public class LocalizationSourceOptions
{
    /// <summary>
    /// Json dosya kaynağı kullanılacak mı?
    /// </summary>
    public bool UseJsonFiles { get; set; } = true;

    /// <summary>
    /// Json dosyalarının bulunduğu dizin
    /// </summary>
    public string JsonFilesDirectory { get; set; } = "Localization";

    /// <summary>
    /// Veritabanı kaynağı kullanılacak mı?
    /// </summary>
    public bool UseDatabase { get; set; } = false;

    /// <summary>
    /// Resource dosyaları kullanılacak mı?
    /// </summary>
    public bool UseResourceFiles { get; set; } = false;

    /// <summary>
    /// Çeviri önbelleği etkin mi?
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Önbellek süresi (dakika)
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 30;
}
