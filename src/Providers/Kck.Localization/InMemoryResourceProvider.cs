using System.Collections.Concurrent;
using Kck.Localization.Abstractions;

namespace Kck.Localization;

/// <summary>
/// In-memory resource provider for testing and development scenarios.
/// </summary>
public sealed class InMemoryResourceProvider : IResourceProvider
{
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _resources = new();

    public InMemoryResourceProvider() { }

    public InMemoryResourceProvider(IDictionary<string, Dictionary<string, string>> resources)
    {
        foreach (var (culture, dict) in resources)
            _resources[culture] = dict.AsReadOnly();
    }

    /// <summary>Adds or replaces all strings for a given culture. Useful for test setup.</summary>
    public void SetStrings(string culture, IDictionary<string, string> strings) =>
        _resources[culture] = strings.AsReadOnly();

    public Task<string?> GetStringAsync(string key, string culture, CancellationToken ct = default)
    {
        if (_resources.TryGetValue(culture, out var dict) && dict.TryGetValue(key, out var value))
            return Task.FromResult<string?>(value);

        return Task.FromResult<string?>(null);
    }

    public Task<IReadOnlyDictionary<string, string>> GetAllStringsAsync(string culture, CancellationToken ct = default)
    {
        if (_resources.TryGetValue(culture, out var dict))
            return Task.FromResult(dict);

        return Task.FromResult<IReadOnlyDictionary<string, string>>(
            new Dictionary<string, string>().AsReadOnly());
    }

    public Task<bool> KeyExistsAsync(string key, string culture, CancellationToken ct = default)
    {
        var exists = _resources.TryGetValue(culture, out var dict) && dict.ContainsKey(key);
        return Task.FromResult(exists);
    }

    public Task ReloadAsync(CancellationToken ct = default) => Task.CompletedTask;
}
