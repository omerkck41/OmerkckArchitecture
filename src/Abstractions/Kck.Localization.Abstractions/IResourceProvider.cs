namespace Kck.Localization.Abstractions;

/// <summary>
/// Loads localized resources from a specific source (JSON files, YAML files, database, etc.).
/// </summary>
public interface IResourceProvider
{
    /// <summary>Provider execution priority. Lower values are queried first. Default: 100.</summary>
    int Priority => 100;

    /// <summary>Indicates whether this provider supports dynamic reload at runtime.</summary>
    bool SupportsDynamicReload => false;

    /// <summary>Gets a localized string by key and culture.</summary>
    Task<string?> GetStringAsync(string key, string culture, CancellationToken ct = default);

    /// <summary>Gets all key-value pairs for a culture.</summary>
    Task<IReadOnlyDictionary<string, string>> GetAllStringsAsync(string culture, CancellationToken ct = default);

    /// <summary>Checks if a key exists for the given culture.</summary>
    Task<bool> KeyExistsAsync(string key, string culture, CancellationToken ct = default);

    /// <summary>Reloads resources from the underlying source.</summary>
    Task ReloadAsync(CancellationToken ct = default);
}
