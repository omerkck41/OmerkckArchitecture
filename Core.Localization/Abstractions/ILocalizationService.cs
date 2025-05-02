using System.Globalization;

namespace Core.Localization.Abstractions;

/// <summary>
/// Main interface for localization operations with async support
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets a localized string value asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">Optional culture, defaults to current culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Localized string</returns>
    Task<string> GetStringAsync(string key, CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a localized string from a specific section asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="section">Section name</param>
    /// <param name="culture">Optional culture, defaults to current culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Localized string</returns>
    Task<string> GetStringAsync(string key, string section, CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a localized string with format arguments asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    Task<string> GetStringAsync(string key, params object[] args);

    /// <summary>
    /// Gets a localized string with format arguments and specific culture asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">Target culture</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    Task<string> GetStringAsync(string key, CultureInfo culture, params object[] args);

    /// <summary>
    /// Gets a localized string from a specific section with format arguments asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="section">Section name</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    Task<string> GetStringAsync(string key, string section, params object[] args);

    /// <summary>
    /// Gets a localized string from a specific section with format arguments and specific culture asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="section">Section name</param>
    /// <param name="culture">Target culture</param>
    /// <param name="args">Format arguments</param>
    /// <returns>Formatted localized string</returns>
    Task<string> GetStringAsync(string key, string section, CultureInfo culture, params object[] args);

    /// <summary>
    /// Tries to get a localized string asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="value">Output parameter for the localized string</param>
    /// <param name="culture">Optional culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if found, false otherwise</returns>
    Task<(bool success, string? value)> TryGetStringAsync(string key, CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to get a localized string from a specific section asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="section">Section name</param>
    /// <param name="value">Output parameter for the localized string</param>
    /// <param name="culture">Optional culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if found, false otherwise</returns>
    Task<(bool success, string? value)> TryGetStringAsync(string key, string section, CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all localized strings for a specific key across cultures asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="section">Optional section name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of culture to localized string</returns>
    Task<IDictionary<CultureInfo, string>> GetAllStringsAsync(string key, string? section = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available keys in the current culture asynchronously
    /// </summary>
    /// <param name="section">Optional section name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of resource keys</returns>
    Task<IEnumerable<string>> GetAllKeysAsync(string? section = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available keys for a specific culture asynchronously
    /// </summary>
    /// <param name="culture">Target culture</param>
    /// <param name="section">Optional section name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of resource keys</returns>
    Task<IEnumerable<string>> GetAllKeysAsync(CultureInfo culture, string? section = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available sections asynchronously
    /// </summary>
    /// <param name="culture">Optional target culture, defaults to current culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of section names</returns>
    Task<IEnumerable<string>> GetAllSectionsAsync(CultureInfo? culture = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all supported cultures asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of supported cultures</returns>
    Task<IEnumerable<CultureInfo>> GetSupportedCulturesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a localized string value for feature-based localization
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="section">Section name</param>
    /// <param name="culture">Optional culture, defaults to current culture</param>
    /// <returns>Localized string</returns>
    [Obsolete("Use GetStringAsync method instead")]
    Task<string> GetLocalizedAsync(string key, string section, CultureInfo? culture = null);
}
