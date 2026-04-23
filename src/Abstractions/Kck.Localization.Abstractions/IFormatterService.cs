namespace Kck.Localization.Abstractions;

/// <summary>
/// Culture-aware formatting for dates, numbers, and currency.
/// </summary>
public interface IFormatterService
{
    /// <summary>Formats a date according to the specified culture.</summary>
    string FormatDate(DateTime value, string culture, string? format = null);

    /// <summary>Formats a number according to the specified culture.</summary>
    string FormatNumber(decimal number, string culture, string? format = null);

    /// <summary>Formats a currency amount according to the specified culture.</summary>
    string FormatCurrency(decimal amount, string culture, string? currencyCode = null);
}
