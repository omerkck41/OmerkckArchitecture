using Core.Localization.Abstract;
using System.Globalization;

namespace Core.Localization.Concrete;

/// <summary>
/// Tarih ve zaman formatlamaları için asenkron servis implementasyonu.
/// </summary>
public class DateTimeServiceAsync : IDateTimeServiceAsync
{
    /// <inheritdoc />
    public Task<string> FormatDateTimeAsync(DateTime dateTime, string? format = null)
    {
        // Eğer format null ise varsayılanın kullanılması
        format ??= CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
        string formatted = dateTime.ToString(format, CultureInfo.CurrentCulture);
        return Task.FromResult(formatted);
    }

    /// <inheritdoc />
    public Task<string> FormatDateTimeAsync(DateTime dateTime, CultureInfo culture, string? format = null)
    {
        // Belirtilen kültürün formatını kullanır
        format ??= culture.DateTimeFormat.ShortDatePattern;
        string formatted = dateTime.ToString(format, culture);
        return Task.FromResult(formatted);
    }

    /// <inheritdoc />
    public Task<DateTimeFormatInfo> GetDateTimeFormatInfoAsync()
    {
        // Şu anki kültür bilgisine göre format bilgisini döndürür
        DateTimeFormatInfo info = CultureInfo.CurrentCulture.DateTimeFormat;
        return Task.FromResult(info);
    }

    /// <inheritdoc />
    public Task<DateTimeFormatInfo> GetDateTimeFormatInfoAsync(CultureInfo culture)
    {
        // Verilen kültür bilgisine göre format bilgisini döndürür
        DateTimeFormatInfo info = culture.DateTimeFormat;
        return Task.FromResult(info);
    }
}