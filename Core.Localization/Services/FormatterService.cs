using Core.Localization.Abstractions;
using System.Globalization;

namespace Core.Localization.Services;

/// <summary>
/// Implementation of formatting services for various data types with async support
/// </summary>
public class FormatterService : IFormatterService
{
    /// <inheritdoc/>
    public Task<string> FormatDateAsync(DateTime date, string? format = null, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= CultureInfo.CurrentCulture;
        var result = date.ToString(format ?? culture.DateTimeFormat.ShortDatePattern, culture);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<string> FormatNumberAsync(decimal number, string? format = null, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= CultureInfo.CurrentCulture;
        var result = number.ToString(format ?? "N2", culture);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<string> FormatCurrencyAsync(decimal amount, string? currencyCode = null, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= CultureInfo.CurrentCulture;

        string result;
        if (currencyCode != null)
        {
            // Create a custom NumberFormatInfo with the specified currency
            var numberFormat = (NumberFormatInfo)culture.NumberFormat.Clone();
            numberFormat.CurrencySymbol = currencyCode;
            result = amount.ToString("C", numberFormat);
        }
        else
        {
            result = amount.ToString("C", culture);
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<string> FormatPercentageAsync(decimal value, int decimals = 2, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= CultureInfo.CurrentCulture;
        var format = $"P{decimals}";
        var result = value.ToString(format, culture);
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<DateTime?> ParseDateAsync(string dateString, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= CultureInfo.CurrentCulture;

        if (DateTime.TryParse(dateString, culture, DateTimeStyles.None, out var result))
        {
            return Task.FromResult<DateTime?>(result);
        }

        // Try parsing with various standard formats
        string[] formats = {
            culture.DateTimeFormat.ShortDatePattern,
            culture.DateTimeFormat.LongDatePattern,
            culture.DateTimeFormat.ShortTimePattern,
            culture.DateTimeFormat.LongTimePattern,
            culture.DateTimeFormat.FullDateTimePattern,
            "yyyy-MM-dd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ"
        };

        if (DateTime.TryParseExact(dateString, formats, culture, DateTimeStyles.None, out result))
        {
            return Task.FromResult<DateTime?>(result);
        }

        return Task.FromResult<DateTime?>(null);
    }

    /// <inheritdoc/>
    public Task<decimal?> ParseNumberAsync(string numberString, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= CultureInfo.CurrentCulture;

        if (decimal.TryParse(numberString, NumberStyles.Number, culture, out var result))
        {
            return Task.FromResult<decimal?>(result);
        }

        return Task.FromResult<decimal?>(null);
    }

    /// <inheritdoc/>
    public Task<decimal?> ParseCurrencyAsync(string currencyString, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= CultureInfo.CurrentCulture;

        // Remove any currency symbols and spaces
        var cleanedString = currencyString.Trim();
        foreach (var symbol in new[] { culture.NumberFormat.CurrencySymbol, "$", "€", "£", "¥", "₺" })
        {
            cleanedString = cleanedString.Replace(symbol, "");
        }

        cleanedString = cleanedString.Trim();

        if (decimal.TryParse(cleanedString, NumberStyles.Currency, culture, out var result))
        {
            return Task.FromResult<decimal?>(result);
        }

        return Task.FromResult<decimal?>(null);
    }
}
