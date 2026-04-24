using System.Collections.Concurrent;
using System.Text.Json;
using Kck.Localization.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kck.Localization.Json;

public sealed partial class JsonResourceProvider(
    IOptionsMonitor<LocalizationOptions> options,
    ILogger<JsonResourceProvider> logger,
    int priority = 100) : IResourceProvider
{
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _cache = new();
    private readonly LocalizationOptions _options = options.CurrentValue;

    public int Priority => priority;
    public bool SupportsDynamicReload => true;

    public async Task<string?> GetStringAsync(string key, string culture, CancellationToken ct = default)
    {
        var resources = await LoadResourcesAsync(culture, ct).ConfigureAwait(false);
        return resources.GetValueOrDefault(key);
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAllStringsAsync(string culture, CancellationToken ct = default)
    {
        return await LoadResourcesAsync(culture, ct).ConfigureAwait(false);
    }

    public async Task<bool> KeyExistsAsync(string key, string culture, CancellationToken ct = default)
    {
        var resources = await LoadResourcesAsync(culture, ct).ConfigureAwait(false);
        return resources.ContainsKey(key);
    }

    public Task ReloadAsync(CancellationToken ct = default)
    {
        _cache.Clear();
        return Task.CompletedTask;
    }

    private async Task<IReadOnlyDictionary<string, string>> LoadResourcesAsync(string culture, CancellationToken ct)
    {
        if (_options.EnableCaching && _cache.TryGetValue(culture, out var cached))
            return cached;

        if (!IsValidCulture(culture))
            return new Dictionary<string, string>();

        var filePath = Path.Join(_options.ResourcePath, $"{culture}.json");
        if (!File.Exists(filePath))
        {
            LogResourceFileNotFound(logger, filePath);
            return new Dictionary<string, string>();
        }

        var json = await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
        var dict = FlattenJson(json);

        if (_options.EnableCaching)
            _cache[culture] = dict;

        return dict;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Resource file not found: {FilePath}")]
    private static partial void LogResourceFileNotFound(ILogger logger, string filePath);

    private static Dictionary<string, string> FlattenJson(string json)
    {
        var result = new Dictionary<string, string>();
        using var doc = JsonDocument.Parse(json);
        FlattenElement(doc.RootElement, "", result);
        return result;
    }

    private static bool IsValidCulture(string culture) =>
        !string.IsNullOrWhiteSpace(culture) &&
        !culture.Contains("..") &&
        culture.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

    private static void FlattenElement(JsonElement element, string prefix, Dictionary<string, string> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    FlattenElement(property.Value, key, result);
                }
                break;
            case JsonValueKind.String:
                result[prefix] = element.GetString() ?? string.Empty;
                break;
            default:
                result[prefix] = element.ToString();
                break;
        }
    }
}
