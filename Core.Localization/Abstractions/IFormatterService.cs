using System.Globalization;

namespace Core.Localization.Abstractions;

/// <summary>
/// Provides formatting services for different data types with async support
/// </summary>
public interface IFormatterService
{
    /// <summary>
    /// Formats a date according to culture-specific rules asynchronously
    /// </summary>
    /// <param name="date">The date to format</param>
    /// <param name="format">Optional format string</param>
    /// <param name="culture">Optional culture, defaults to current culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Formatted date string</returns>
    Task<string> FormatDateAsync(DateTime date, string? format = null, CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats a number according to culture-specific rules asynchronously
    /// </summary>
    /// <param name="number">The number to format</param>
    /// <param name="format">Optional format string</param>
    /// <param name="culture">Optional culture, defaults to current culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Formatted number string</returns>
    Task<string> FormatNumberAsync(decimal number, string? format = null, CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats currency according to culture-specific rules asynchronously
    /// </summary>
    /// <param name="amount">The amount to format</param>
    /// <param name="currencyCode">Optional currency code (e.g., USD, EUR)</param>
    /// <param name="culture">Optional culture, defaults to current culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Formatted currency string</returns>
    Task<string> FormatCurrencyAsync(decimal amount, string? currencyCode = null, CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Formats a percentage according to culture-specific rules asynchronously
    /// </summary>
    /// <param name="value">The value to format as percentage</param>
    /// <param name="decimals">Number of decimal places</param>
    /// <param name="culture">Optional culture, defaults to current culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Formatted percentage string</returns>
    Task<string> FormatPercentageAsync(decimal value, int decimals = 2, CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses a culture-specific date string asynchronously
    /// </summary>
    /// <param name="dateString">The date string to parse</param>
    /// <param name="culture">Optional culture, defaults to current culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parsed DateTime or null if parsing fails</returns>
    Task<DateTime?> ParseDateAsync(string dateString, CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses a culture-specific number string asynchronously
    /// </summary>
    /// <param name="numberString">The number string to parse</param>
    /// <param name="culture">Optional culture, defaults to current culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parsed decimal or null if parsing fails</returns>
    Task<decimal?> ParseNumberAsync(string numberString, CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parses a culture-specific currency string asynchronously
    /// </summary>
    /// <param name="currencyString">The currency string to parse</param>
    /// <param name="culture">Optional culture, defaults to current culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parsed decimal or null if parsing fails</returns>
    Task<decimal?> ParseCurrencyAsync(string currencyString, CultureInfo? culture = null, CancellationToken cancellationToken = default);
}
