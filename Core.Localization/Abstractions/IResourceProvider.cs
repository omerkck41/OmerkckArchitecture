using System.Globalization;

namespace Core.Localization.Abstractions;

/// <summary>
/// Defines the contract for resource providers that supply localization data
/// </summary>
public interface IResourceProvider
{
    /// <summary>
    /// Gets a localized string value for the specified key and culture asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">The target culture</param>
    /// <param name="section">Optional section name for feature-based localization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Localized string value or null if not found</returns>
    Task<string?> GetStringAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a localized object value for the specified key and culture asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">The target culture</param>
    /// <param name="section">Optional section name for feature-based localization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Localized object value or null if not found</returns>
    Task<object?> GetResourceAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available keys for a given culture asynchronously
    /// </summary>
    /// <param name="culture">The target culture</param>
    /// <param name="section">Optional section name for feature-based localization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of available keys</returns>
    Task<IEnumerable<string>> GetAllKeysAsync(CultureInfo culture, string? section = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available sections for a given culture asynchronously
    /// </summary>
    /// <param name="culture">The target culture</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of available sections</returns>
    Task<IEnumerable<string>> GetAllSectionsAsync(CultureInfo culture, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the provider has a specific key for a given culture asynchronously
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">The target culture</param>
    /// <param name="section">Optional section name for feature-based localization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the key exists, false otherwise</returns>
    Task<bool> HasKeyAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the provider's priority for conflict resolution
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Gets whether the provider supports dynamic reloading
    /// </summary>
    bool SupportsDynamicReload { get; }

    /// <summary>
    /// Reloads the resource provider's data asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ReloadAsync(CancellationToken cancellationToken = default);
}
