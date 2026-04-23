namespace Kck.Localization.Abstractions;

/// <summary>
/// High-level localization service with fallback, pluralization, and formatting.
/// </summary>
public interface ILocalizationService
{
    /// <summary>Gets a localized string. Falls back to default culture if not found.</summary>
    Task<string> GetStringAsync(string key, CancellationToken ct = default);

    /// <summary>Gets a localized string for a specific culture.</summary>
    Task<string> GetStringAsync(string key, string culture, CancellationToken ct = default);

    /// <summary>Gets a localized string with format arguments.</summary>
    Task<string> GetStringAsync(string key, string culture, object[] args, CancellationToken ct = default);

    /// <summary>Tries to get a localized string. Returns null instead of the key on miss.</summary>
    Task<string?> TryGetStringAsync(string key, string culture, CancellationToken ct = default);

    /// <summary>Gets a pluralized string based on count. Uses ICU plural categories.</summary>
    Task<string> GetPluralStringAsync(string key, int count, string culture, CancellationToken ct = default);

    /// <summary>Gets all localized strings for the specified culture.</summary>
    Task<IReadOnlyDictionary<string, string>> GetAllStringsAsync(string culture, CancellationToken ct = default);

    /// <summary>Gets all known keys across all providers for the specified culture.</summary>
    Task<IReadOnlyList<string>> GetAllKeysAsync(string culture, CancellationToken ct = default);

    /// <summary>Reloads all providers that support dynamic reload.</summary>
    Task ReloadAsync(CancellationToken ct = default);
}
