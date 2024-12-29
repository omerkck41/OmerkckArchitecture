using System.Globalization;

namespace Core.ToolKit.Localization;

public static class CurrencyFormatter
{
    /// <summary>
    /// Formats a decimal value as currency according to the default culture with optional symbol override.
    /// </summary>
    /// <param name="amount">The monetary value to format.</param>
    /// <param name="currencySymbol">Optional currency symbol to override default.</param>
    /// <returns>Formatted currency string.</returns>
    public static string Format(decimal amount, string? currencySymbol = null)
    {
        var cultureInfo = new CultureInfo(LocalizationHelper.DefaultCulture);
        var formatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();

        if (!string.IsNullOrEmpty(currencySymbol))
            formatInfo.CurrencySymbol = currencySymbol;

        return string.Format(formatInfo, "{0:C}", amount);
    }

    /// <summary>
    /// Parses a currency string into a decimal value according to the default culture.
    /// </summary>
    /// <param name="currencyString">The currency string to parse.</param>
    /// <returns>Parsed decimal value.</returns>
    public static decimal Parse(string currencyString)
    {
        if (string.IsNullOrWhiteSpace(currencyString))
            throw new ArgumentException("Currency string cannot be null or empty.", nameof(currencyString));

        var cultureInfo = new CultureInfo(LocalizationHelper.DefaultCulture);
        if (decimal.TryParse(currencyString, NumberStyles.Currency, cultureInfo, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid currency string: {currencyString}");
    }
}