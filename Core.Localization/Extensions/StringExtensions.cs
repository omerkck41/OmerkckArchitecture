using Core.Localization.Abstractions;
using System.Globalization;

namespace Core.Localization.Extensions;

/// <summary>
/// Extension methods for string localization with async support
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Gets a localized string using the specified key asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="localizationService">The localization service</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Localized string</returns>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, CancellationToken cancellationToken = default)
    {
        return localizationService.GetStringAsync(key, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a localized string using the specified key and culture asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="localizationService">The localization service</param>
    /// <param name="culture">The culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Localized string</returns>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        return localizationService.GetStringAsync(key, culture, cancellationToken);
    }

    /// <summary>
    /// Gets a localized string with format arguments asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="localizationService">The localization service</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, params object[] args)
    {
        return localizationService.GetStringAsync(key, args);
    }

    /// <summary>
    /// Gets a localized string with format arguments and specific culture asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="localizationService">The localization service</param>
    /// <param name="culture">The culture</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, CultureInfo culture, params object[] args)
    {
        return localizationService.GetStringAsync(key, culture, args);
    }

    /// <summary>
    /// Gets a localized string from a specific section asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="localizationService">The localization service</param>
    /// <param name="section">Section name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Localized string</returns>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, string section, CancellationToken cancellationToken = default)
    {
        return localizationService.GetStringAsync(key, section, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a localized string from a specific section with culture asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="localizationService">The localization service</param>
    /// <param name="section">Section name</param>
    /// <param name="culture">The culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Localized string</returns>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, string section, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        return localizationService.GetStringAsync(key, section, culture, cancellationToken);
    }

    /// <summary>
    /// Gets a localized string from a specific section with format arguments asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="localizationService">The localization service</param>
    /// <param name="section">Section name</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, string section, params object[] args)
    {
        return localizationService.GetStringAsync(key, section, args);
    }

    /// <summary>
    /// Gets a localized string from a specific section with format arguments and specific culture asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="localizationService">The localization service</param>
    /// <param name="section">Section name</param>
    /// <param name="culture">The culture</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, string section, CultureInfo culture, params object[] args)
    {
        return localizationService.GetStringAsync(key, section, culture, args);
    }

    /// <summary>
    /// Tries to get a localized string asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="localizationService">The localization service</param>
    /// <param name="culture">Optional culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Localization result containing success status and value if found</returns>
    public static Task<LocalizationResult> TryLocalizeAsync(this string key, ILocalizationService localizationService, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        return localizationService.TryGetStringAsync(key, culture, cancellationToken);
    }

    /// <summary>
    /// Tries to get a localized string with specific section asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="localizationService">The localization service</param>
    /// <param name="section">Section name</param>
    /// <param name="culture">Optional culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Localization result containing success status and value if found</returns>
    public static Task<LocalizationResult> TryLocalizeAsync(this string key, ILocalizationService localizationService, string section, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        return localizationService.TryGetStringAsync(key, section, culture, cancellationToken);
    }
}
