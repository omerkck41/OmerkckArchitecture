using System.Collections.Concurrent;
using Kck.Localization.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Kck.Localization.Yaml;

public sealed partial class YamlResourceProvider(
    IOptionsMonitor<LocalizationOptions> options,
    ILogger<YamlResourceProvider> logger,
    int priority = 100) : IResourceProvider
{
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _cache = new();
    private readonly LocalizationOptions _options = options.CurrentValue;

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

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

        var filePath = Path.Join(_options.ResourcePath, $"{culture}.yaml");
        if (!File.Exists(filePath))
            filePath = Path.Join(_options.ResourcePath, $"{culture}.yml");

        if (!File.Exists(filePath))
        {
            LogYamlResourceNotFound(logger, culture);
            return new Dictionary<string, string>();
        }

        var yaml = await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
        var dict = FlattenYaml(yaml);

        if (_options.EnableCaching)
            _cache[culture] = dict;

        return dict;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "YAML resource file not found for culture: {Culture}")]
    private static partial void LogYamlResourceNotFound(ILogger logger, string culture);

    private static Dictionary<string, string> FlattenYaml(string yaml)
    {
        var result = new Dictionary<string, string>();
        var data = Deserializer.Deserialize<Dictionary<string, object>>(yaml);
        if (data is not null)
            Flatten(data, "", result);
        return result;
    }

    private static bool IsValidCulture(string culture) =>
        !string.IsNullOrWhiteSpace(culture) &&
        !culture.Contains("..") &&
        culture.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

    private static void Flatten(Dictionary<string, object> source, string prefix, Dictionary<string, string> result)
    {
        foreach (var kvp in source)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";
            if (kvp.Value is Dictionary<object, object> nested)
            {
                var typed = nested.ToDictionary(k => k.Key.ToString()!, v => v.Value);
                Flatten(typed, key, result);
            }
            else
            {
                result[key] = kvp.Value?.ToString() ?? "";
            }
        }
    }
}
