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
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, CancellationToken cancellationToken = default)
    {
        return localizationService.GetStringAsync(key, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a localized string using the specified key and culture asynchronously
    /// </summary>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        return localizationService.GetStringAsync(key, culture, cancellationToken);
    }

    /// <summary>
    /// Gets a localized string with format arguments asynchronously
    /// </summary>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, params object[] args)
    {
        return localizationService.GetStringAsync(key, args);
    }

    /// <summary>
    /// Gets a localized string with format arguments and specific culture asynchronously
    /// </summary>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, CultureInfo culture, params object[] args)
    {
        return localizationService.GetStringAsync(key, culture, args);
    }

    /// <summary>
    /// Gets a localized string from a specific section asynchronously
    /// </summary>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, string section, CancellationToken cancellationToken = default)
    {
        return localizationService.GetStringAsync(key, section, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a localized string from a specific section with culture asynchronously
    /// </summary>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, string section, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        return localizationService.GetStringAsync(key, section, culture, cancellationToken);
    }

    /// <summary>
    /// Gets a localized string from a specific section with format arguments asynchronously
    /// </summary>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, string section, params object[] args)
    {
        return localizationService.GetStringAsync(key, section, args);
    }

    /// <summary>
    /// Gets a localized string from a specific section with format arguments and specific culture asynchronously
    /// </summary>
    public static Task<string> LocalizeAsync(this string key, ILocalizationService localizationService, string section, CultureInfo culture, params object[] args)
    {
        return localizationService.GetStringAsync(key, section, culture, args);
    }

    /// <summary>
    /// Tries to get a localized string asynchronously
    /// </summary>
    public static async Task<(bool success, string? value)> TryLocalizeAsync(this string key, ILocalizationService localizationService, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        return await localizationService.TryGetStringAsync(key, culture, cancellationToken);
    }

    /// <summary>
    /// Tries to get a localized string with specific section asynchronously
    /// </summary>
    public static async Task<(bool success, string? value)> TryLocalizeAsync(this string key, ILocalizationService localizationService, string section, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        return await localizationService.TryGetStringAsync(key, section, culture, cancellationToken);
    }
}
