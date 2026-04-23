using System.Globalization;
using Kck.Localization.Abstractions;

namespace Kck.Localization;

/// <summary>
/// Culture-aware formatting service for dates, numbers, and currency.
/// </summary>
public sealed class FormatterService : IFormatterService
{
    public string FormatDate(DateTime value, string culture, string? format = null)
    {
        var cultureInfo = GetCultureInfoSafe(culture);
        return value.ToString(format ?? "g", cultureInfo);
    }

    public string FormatNumber(decimal number, string culture, string? format = null)
    {
        var cultureInfo = GetCultureInfoSafe(culture);
        return number.ToString(format ?? "N2", cultureInfo);
    }

    public string FormatCurrency(decimal amount, string culture, string? currencyCode = null)
    {
        var cultureInfo = GetCultureInfoSafe(culture);

        if (currencyCode is not null)
        {
            var formatted = amount.ToString("N2", cultureInfo);
            return $"{currencyCode} {formatted}";
        }

        return amount.ToString("C", cultureInfo);
    }

    private static CultureInfo GetCultureInfoSafe(string culture)
    {
        try
        {
            return CultureInfo.GetCultureInfo(culture);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }
}
