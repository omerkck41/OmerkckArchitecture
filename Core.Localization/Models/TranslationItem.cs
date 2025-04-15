namespace Core.Localization.Models;

/// <summary>
/// Çeviri bilgisi
/// </summary>
public class TranslationItem
{
    /// <summary>
    /// Çeviri anahtarı
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Çeviri değeri
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
