using System.Globalization;

namespace Core.Localization.Abstract;

/// <summary>
/// Para birimi işlemleri için servis sözleşmesi (asenkron versiyon)
/// </summary>
public interface ICurrencyServiceAsync
{
    /// <summary>
    /// Belirtilen tutarı mevcut kültüre göre asenkron formatlar.
    /// </summary>
    Task<string> FormatCurrencyAsync(decimal amount);

    /// <summary>
    /// Belirtilen tutarı belirtilen kültüre göre asenkron formatlar.
    /// </summary>
    Task<string> FormatCurrencyAsync(decimal amount, CultureInfo culture);

    /// <summary>
    /// Belirtilen tutarı belirtilen para birimine dönüştürür (asenkron).
    /// </summary>
    /// <param name="amount">Dönüştürülecek tutar</param>
    /// <param name="fromCurrency">Kaynak para birimi</param>
    /// <param name="toCurrency">Hedef para birimi</param>
    /// <returns>Dönüştürülmüş tutar</returns>
    Task<decimal> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency);

    /// <summary>
    /// Desteklenen para birimlerini döndürür.
    /// </summary>
    IEnumerable<string> GetSupportedCurrencies();
}