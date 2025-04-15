using System.Globalization;

namespace Core.Localization.Abstract;

/// <summary>
/// Tarih ve zaman formatlamaları için asenkron servis sözleşmesi.
/// </summary>
public interface IDateTimeServiceAsync
{
    /// <summary>
    /// Belirtilen tarihi, mevcut kültüre göre asenkron olarak formatlar.
    /// </summary>
    /// <param name="dateTime">Formatlanacak tarih</param>
    /// <param name="format">Format şablonu (null ise standart format kullanılır)</param>
    /// <returns>Formatlanmış tarih</returns>
    Task<string> FormatDateTimeAsync(DateTime dateTime, string? format = null);

    /// <summary>
    /// Belirtilen tarihi, belirtilen kültüre göre asenkron olarak formatlar.
    /// </summary>
    /// <param name="dateTime">Formatlanacak tarih</param>
    /// <param name="culture">Kültür bilgisi</param>
    /// <param name="format">Format şablonu (null ise standart format kullanılır)</param>
    /// <returns>Formatlanmış tarih</returns>
    Task<string> FormatDateTimeAsync(DateTime dateTime, CultureInfo culture, string? format = null);

    /// <summary>
    /// Mevcut kültüre göre tarih-zaman format bilgisini asenkron olarak döndürür.
    /// </summary>
    /// <returns>Tarih-zaman format bilgileri</returns>
    Task<DateTimeFormatInfo> GetDateTimeFormatInfoAsync();

    /// <summary>
    /// Belirtilen kültüre göre tarih-zaman format bilgisini asenkron olarak döndürür.
    /// </summary>
    /// <param name="culture">Kültür bilgisi</param>
    /// <returns>Tarih-zaman format bilgileri</returns>
    Task<DateTimeFormatInfo> GetDateTimeFormatInfoAsync(CultureInfo culture);
}