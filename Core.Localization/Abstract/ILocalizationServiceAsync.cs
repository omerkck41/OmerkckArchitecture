using System.Globalization;

namespace Core.Localization.Abstract;

/// <summary>
/// Yerelleştirme servisi için asenkron sözleşme.
/// </summary>
public interface ILocalizationServiceAsync
{
    /// <summary>
    /// Belirtilen anahtar için yerelleştirilmiş metni asenkron olarak getirir.
    /// </summary>
    Task<string> GetStringAsync(string key);

    /// <summary>
    /// Belirtilen anahtar ve format parametreleri ile yerelleştirilmiş metni asenkron olarak getirir.
    /// </summary>
    Task<string> GetStringAsync(string key, params object[] args);

    /// <summary>
    /// Mevcut aktif kültürü döndürür.
    /// </summary>
    CultureInfo GetCurrentCulture();

    /// <summary>
    /// Aktif kültürü ayarlar.
    /// </summary>
    void SetCurrentCulture(CultureInfo culture);

    /// <summary>
    /// Kültür koduna göre aktif kültürü ayarlar.
    /// </summary>
    void SetCurrentCulture(string cultureName);

    /// <summary>
    /// Desteklenen tüm kültürleri döndürür.
    /// </summary>
    IEnumerable<CultureInfo> GetSupportedCultures();
}