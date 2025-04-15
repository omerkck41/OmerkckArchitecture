namespace Core.Localization.Models;

/// <summary>
/// Kültür bilgisi
/// </summary>
public class CultureModel
{
    /// <summary>
    /// Kültür kodu (örn. "tr-TR")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Kültür adı
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Kültürün kendi dilindeki adı
    /// </summary>
    public string NativeName { get; set; } = string.Empty;

    /// <summary>
    /// İlişkili bayrak kodu (ISO 3166-1 alpha-2)
    /// </summary>
    public string FlagCode { get; set; } = string.Empty;

    /// <summary>
    /// Kültürün dil kodu (örn. "tr")
    /// </summary>
    public string LanguageCode => Code.Split('-')[0];
}