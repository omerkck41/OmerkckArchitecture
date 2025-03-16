using System.Globalization;

namespace Core.ToolKit.Localization;

public static class CurrencyFormatter
{
    private static string _globalCurrencySymbol = string.Empty;

    public static string GlobalCurrencySymbol
    {
        get => _globalCurrencySymbol;
        set => _globalCurrencySymbol = value;
    }

    public static string Format(decimal amount, string? currencySymbol = null, CultureInfo? cultureInfo = null)
    {
        cultureInfo ??= new CultureInfo(LocalizationHelper.DefaultCulture);
        var formatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();

        if (!string.IsNullOrEmpty(currencySymbol))
            formatInfo.CurrencySymbol = currencySymbol;
        else if (!string.IsNullOrEmpty(_globalCurrencySymbol))
            formatInfo.CurrencySymbol = _globalCurrencySymbol;

        return string.Format(formatInfo, "{0:C}", amount);
    }

    public static decimal Parse(string currencyString, CultureInfo? cultureInfo = null)
    {
        if (string.IsNullOrWhiteSpace(currencyString))
            throw new ArgumentException("Currency string cannot be null or empty.", nameof(currencyString));

        cultureInfo ??= new CultureInfo(LocalizationHelper.DefaultCulture);
        if (decimal.TryParse(currencyString, NumberStyles.Currency, cultureInfo, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid currency string: {currencyString}");
    }

    public static string FormatWithCulture(decimal amount, CultureInfo cultureInfo, string? currencySymbol = null)
    {
        var formatInfo = (NumberFormatInfo)cultureInfo.NumberFormat.Clone();

        if (!string.IsNullOrEmpty(currencySymbol))
            formatInfo.CurrencySymbol = currencySymbol;
        else if (!string.IsNullOrEmpty(_globalCurrencySymbol))
            formatInfo.CurrencySymbol = _globalCurrencySymbol;

        return string.Format(formatInfo, "{0:C}", amount);
    }

    public static decimal ParseWithCulture(string currencyString, CultureInfo cultureInfo)
    {
        if (string.IsNullOrWhiteSpace(currencyString))
            throw new ArgumentException("Currency string cannot be null or empty.", nameof(currencyString));

        if (decimal.TryParse(currencyString, NumberStyles.Currency, cultureInfo, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid currency string: {currencyString}");
    }
}