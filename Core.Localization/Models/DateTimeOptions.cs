namespace Core.Localization.Models;

/// <summary>
/// Tarih-zaman ayarları
/// </summary>
public class DateTimeOptions
{
    /// <summary>
    /// Varsayılan tarih formatı
    /// </summary>
    public string DefaultDateFormat { get; set; } = "d";

    /// <summary>
    /// Varsayılan saat formatı
    /// </summary>
    public string DefaultTimeFormat { get; set; } = "t";

    /// <summary>
    /// Varsayılan tarih-saat formatı
    /// </summary>
    public string DefaultDateTimeFormat { get; set; } = "g";
}
