using Core.Localization.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace Core.Localization.Providers;

/// <summary>
/// Provides resources from JSON files with feature-based localization and async support
/// </summary>
public class JsonResourceProvider : ResourceProviderBase, IDisposable
{
    private readonly LocalizationOptions _options;
    private readonly ILogger<JsonResourceProvider> _logger;
    private readonly ConcurrentDictionary<string, Dictionary<string, object>> _resourceCache;
    private readonly List<FileSystemWatcher> _fileWatchers = new();
    private readonly CancellationTokenSource _scanTokenSource = new();
    private readonly Timer? _resourceScanTimer;
    private readonly ConcurrentDictionary<string, string> _sectionNameCache = new();

    public JsonResourceProvider(
        IOptions<LocalizationOptions> options,
        ILogger<JsonResourceProvider> logger,
        int priority = 200) : base(priority)
    {
        _options = options.Value;
        _logger = logger;
        _resourceCache = new ConcurrentDictionary<string, Dictionary<string, object>>();

        // Load resources initially
        LoadResourcesAsync().GetAwaiter().GetResult();

        // Setup auto-reload timer if enabled
        if (_options.EnableAutoDiscovery)
        {
            _resourceScanTimer = new Timer(
                _ => ScanForResourcesAsync(_scanTokenSource.Token).ConfigureAwait(false),
                null,
                _options.AutoReloadInterval,
                _options.AutoReloadInterval);
        }

        // Setup file watchers if enabled
        if (_options.UseFileSystemWatcher)
        {
            InitializeFileWatchers();
        }
    }

    public override bool SupportsDynamicReload => true;

    public override async Task<string?> GetStringAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        var cultureName = GetNormalizedCultureCode(culture);
        var effectiveKey = GetEffectiveKey(key, section);

        foreach (var cacheEntry in _resourceCache)
        {
            // Try to match the section and culture from the cache key
            // Cache keys are in format: "sectionName.cultureName" or "cultureName" for default section
            var cacheKey = cacheEntry.Key;

            if (IsCacheKeyMatch(cacheKey, section, cultureName))
            {
                var resources = cacheEntry.Value;

                if (TryGetNestedValue(resources, effectiveKey, out var value) && value != null)
                {
                    return value.ToString();
                }
            }
        }

        // Try parent culture
        var parentCulture = GetParentCulture(culture);
        if (parentCulture != null)
        {
            return await GetStringAsync(key, parentCulture, section, cancellationToken);
        }

        return null;
    }

    public override async Task<IEnumerable<string>> GetAllKeysAsync(CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        var cultureName = GetNormalizedCultureCode(culture);
        var keys = new HashSet<string>();

        foreach (var cacheEntry in _resourceCache)
        {
            if (IsCacheKeyMatch(cacheEntry.Key, section, cultureName))
            {
                var resources = cacheEntry.Value;
                var allKeys = GetAllKeysFromDictionary(resources, section);
                foreach (var key in allKeys)
                {
                    keys.Add(key);
                }
            }
        }

        return keys;
    }

    public override async Task<IEnumerable<string>> GetAllSectionsAsync(CultureInfo culture, CancellationToken cancellationToken = default)
    {
        var cultureName = GetNormalizedCultureCode(culture);
        var sections = new HashSet<string>();

        foreach (var cacheEntry in _resourceCache)
        {
            var cacheKey = cacheEntry.Key;
            var keyParts = cacheKey.Split('.');

            // Cache keys may be in format:
            // 1. "section.culture" - we want the section part
            // 2. "culture" - default section

            if (keyParts.Length > 1 && keyParts.Last().Equals(cultureName, StringComparison.OrdinalIgnoreCase))
            {
                // If the last part is the culture, then everything before is the section
                var sectionName = string.Join(".", keyParts.Take(keyParts.Length - 1));
                sections.Add(sectionName);
            }
            else if (keyParts.Length == 1 && keyParts[0].Equals(cultureName, StringComparison.OrdinalIgnoreCase))
            {
                // This is a default section
                sections.Add(_options.DefaultSection);
            }
        }

        // Also check for explicitly defined section names in the resources
        foreach (var sectionName in _sectionNameCache.Values)
        {
            sections.Add(sectionName);
        }

        return sections;
    }

    public override async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        await LoadResourcesAsync(cancellationToken);
    }

    private async Task LoadResourcesAsync(CancellationToken cancellationToken = default)
    {
        // Clear existing cache
        _resourceCache.Clear();
        _sectionNameCache.Clear();

        // Scan for resources in all configured paths
        await ScanForResourcesAsync(cancellationToken);
    }

    private async Task ScanForResourcesAsync(CancellationToken cancellationToken)
    {
        foreach (var basePath in _options.ResourcePaths)
        {
            if (!Directory.Exists(basePath))
            {
                _logger.LogWarning("Resource path {Path} does not exist", basePath);
                continue;
            }

            // Find all directories matching the feature pattern
            var featureDirs = Directory.GetDirectories(basePath, "*", SearchOption.AllDirectories)
                .Where(dir => dir.Contains("Resources") && Directory.Exists(Path.Combine(dir, "Locales")))
                .Select(dir => Path.Combine(dir, "Locales"))
                .ToList();

            // Add the base path if it contains a Locales directory
            var baseLocalesPath = Path.Combine(basePath, "Locales");
            if (Directory.Exists(baseLocalesPath))
            {
                featureDirs.Add(baseLocalesPath);
            }

            foreach (var localesDir in featureDirs)
            {
                await LoadResourceFilesFromDirectoryAsync(localesDir, cancellationToken);
            }
        }
    }

    private async Task LoadResourceFilesFromDirectoryAsync(string directory, CancellationToken cancellationToken)
    {
        // We're only interested in JSON files
        var files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            try
            {
                await LoadResourceFileAsync(file, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load resource file {File}", file);
            }
        }
    }

    private async Task LoadResourceFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(filePath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

            // Parse file name to extract section and culture
            // Format expected: section.culture.json (e.g., users.en.json)
            var parts = fileNameWithoutExt.Split('.');

            if (parts.Length < 2)
            {
                _logger.LogWarning("Invalid resource file name format: {FileName}. Expected format: section.culture.json", fileName);
                return;
            }

            var section = parts[0];
            var culture = parts[1];
            var cacheKey = $"{section}.{culture}";

            // Read and parse the JSON file
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);

            // Use JsonDocument for more flexibility
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            // Create a dictionary to hold values
            var resources = new Dictionary<string, object>();

            // Handle section name if present
            if (root.TryGetProperty(_options.SectionKey, out var sectionNameElement) &&
                sectionNameElement.ValueKind == JsonValueKind.String)
            {
                var sectionName = sectionNameElement.GetString();
                if (!string.IsNullOrEmpty(sectionName))
                {
                    _sectionNameCache[section] = sectionName;
                    resources[_options.SectionKey] = sectionName;
                }
            }

            // Process all properties
            foreach (var property in root.EnumerateObject())
            {
                ExtractJsonProperties(property, resources);
            }

            // Store in cache
            _resourceCache[cacheKey] = resources;

            _logger.LogInformation("Loaded resource file {File} with section {Section} and culture {Culture}",
                filePath, section, culture);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading resource file {File}", filePath);
        }
    }

    private void ExtractJsonProperties(JsonProperty property, Dictionary<string, object> target, string? prefix = null)
    {
        var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

        switch (property.Value.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var childProperty in property.Value.EnumerateObject())
                {
                    ExtractJsonProperties(childProperty, target, key);
                }
                break;

            case JsonValueKind.Array:
                var arrayValues = new List<object>();
                foreach (var item in property.Value.EnumerateArray())
                {
                    arrayValues.Add(ConvertJsonElement(item));
                }
                target[key] = arrayValues;
                break;

            case JsonValueKind.String:
                target[key] = property.Value.GetString() ?? string.Empty;
                break;

            case JsonValueKind.Number:
                if (property.Value.TryGetInt32(out int intValue))
                {
                    target[key] = intValue;
                }
                else if (property.Value.TryGetInt64(out long longValue))
                {
                    target[key] = longValue;
                }
                else if (property.Value.TryGetDouble(out double doubleValue))
                {
                    target[key] = doubleValue;
                }
                else
                {
                    target[key] = property.Value.GetRawText();
                }
                break;

            case JsonValueKind.True:
                target[key] = true;
                break;

            case JsonValueKind.False:
                target[key] = false;
                break;

            case JsonValueKind.Null:
                target[key] = null!;
                break;

            default:
                target[key] = property.Value.GetRawText();
                break;
        }
    }

    private object ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new Dictionary<string, object>();
                foreach (var property in element.EnumerateObject())
                {
                    obj[property.Name] = ConvertJsonElement(property.Value);
                }
                return obj;

            case JsonValueKind.Array:
                var array = new List<object>();
                foreach (var item in element.EnumerateArray())
                {
                    array.Add(ConvertJsonElement(item));
                }
                return array;

            case JsonValueKind.String:
                return element.GetString() ?? string.Empty;

            case JsonValueKind.Number:
                if (element.TryGetInt32(out int intValue))
                {
                    return intValue;
                }
                else if (element.TryGetInt64(out long longValue))
                {
                    return longValue;
                }
                else if (element.TryGetDouble(out double doubleValue))
                {
                    return doubleValue;
                }
                return element.GetRawText();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null!;

            default:
                return element.GetRawText();
        }
    }

    private void InitializeFileWatchers()
    {
        foreach (var basePath in _options.ResourcePaths)
        {
            if (!Directory.Exists(basePath))
            {
                continue;
            }

            try
            {
                // Find feature directories with Locales
                var featureDirs = Directory.GetDirectories(basePath, "*", SearchOption.AllDirectories)
                    .Where(dir => dir.Contains("Resources") && Directory.Exists(Path.Combine(dir, "Locales")))
                    .Select(dir => Path.Combine(dir, "Locales"))
                    .ToList();

                // Add the base path if it contains a Locales directory
                var baseLocalesPath = Path.Combine(basePath, "Locales");
                if (Directory.Exists(baseLocalesPath))
                {
                    featureDirs.Add(baseLocalesPath);
                }

                foreach (var localesDir in featureDirs)
                {
                    var watcher = new FileSystemWatcher(localesDir, "*.json")
                    {
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.Size,
                        EnableRaisingEvents = true,
                        IncludeSubdirectories = false
                    };

                    watcher.Changed += OnResourceFileChanged;
                    watcher.Created += OnResourceFileChanged;
                    watcher.Deleted += OnResourceFileChanged;
                    watcher.Renamed += OnResourceFileRenamed;

                    _fileWatchers.Add(watcher);

                    _logger.LogInformation("Initialized file watcher for {Directory} with pattern *.json", localesDir);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize file watchers for path {Path}", basePath);
            }
        }
    }

    private async void OnResourceFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("Resource file changed: {FilePath}", e.FullPath);

        try
        {
            // Small delay to allow file system to complete writing the file
            await Task.Delay(100);
            await LoadResourceFileAsync(e.FullPath, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling resource file change for {File}", e.FullPath);
        }
    }

    private async void OnResourceFileRenamed(object sender, RenamedEventArgs e)
    {
        _logger.LogInformation("Resource file renamed from {OldPath} to {NewPath}", e.OldFullPath, e.FullPath);

        try
        {
            // Remove old entry from cache based on old filename
            var oldFileName = Path.GetFileNameWithoutExtension(e.OldName);
            var oldParts = oldFileName.Split('.');
            if (oldParts.Length >= 2)
            {
                var oldSection = oldParts[0];
                var oldCulture = oldParts[1];
                var oldCacheKey = $"{oldSection}.{oldCulture}";
                _resourceCache.TryRemove(oldCacheKey, out _);
            }

            // Load the renamed file
            await LoadResourceFileAsync(e.FullPath, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling resource file rename from {OldFile} to {NewFile}",
                e.OldFullPath, e.FullPath);
        }
    }

    private bool TryGetNestedValue(Dictionary<string, object> resources, string key, out object? value)
    {
        value = null;
        var keys = key.Split('.');

        // Starting point
        Dictionary<string, object>? current = resources;

        // Navigate through nested levels
        for (int i = 0; i < keys.Length - 1; i++)
        {
            var k = keys[i];

            if (current!.TryGetValue(k, out var nextValue) && nextValue is Dictionary<string, object> dict)
            {
                current = dict;
            }
            else if (current!.TryGetValue(k, out nextValue) && nextValue is IDictionary<string, object> typedDict)
            {
                current = typedDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            else
            {
                return false;
            }
        }

        // Get the final value
        var lastKey = keys.Last();
        if (current!.TryGetValue(lastKey, out var finalValue))
        {
            value = finalValue;
            return true;
        }

        return false;
    }

    private IEnumerable<string> GetAllKeysFromDictionary(Dictionary<string, object> resources, string? prefix = null)
    {
        var keys = new List<string>();

        foreach (var kvp in resources)
        {
            var key = kvp.Key;

            // Skip section name key
            if (key.Equals(_options.SectionKey, StringComparison.OrdinalIgnoreCase))
                continue;

            var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";

            if (kvp.Value is Dictionary<string, object> dict)
            {
                keys.AddRange(GetAllKeysFromDictionary(dict, fullKey));
            }
            else if (kvp.Value is IDictionary<string, object> typedDict)
            {
                var convertedDict = typedDict.ToDictionary(d => d.Key, d => d.Value);
                keys.AddRange(GetAllKeysFromDictionary(convertedDict, fullKey));
            }
            else
            {
                keys.Add(fullKey);
            }
        }

        return keys;
    }

    /// <summary>
    /// Check if the cache key matches the given section and culture
    /// </summary>
    private bool IsCacheKeyMatch(string cacheKey, string? section, string cultureName)
    {
        // Cache keys are in format: "sectionName.cultureName"
        var keyParts = cacheKey.Split('.');

        // If it's just a culture key (default section)
        if (keyParts.Length == 1)
        {
            return string.IsNullOrEmpty(section) &&
                   keyParts[0].Equals(cultureName, StringComparison.OrdinalIgnoreCase);
        }

        // If it's a section.culture key
        if (keyParts.Length == 2)
        {
            var keySection = keyParts[0];
            var keyCulture = keyParts[1];

            return keyCulture.Equals(cultureName, StringComparison.OrdinalIgnoreCase) &&
                   (string.IsNullOrEmpty(section) || keySection.Equals(section, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    public void Dispose()
    {
        _scanTokenSource.Cancel();
        _resourceScanTimer?.Dispose();

        foreach (var watcher in _fileWatchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Changed -= OnResourceFileChanged;
            watcher.Created -= OnResourceFileChanged;
            watcher.Deleted -= OnResourceFileChanged;
            watcher.Renamed -= OnResourceFileRenamed;
            watcher.Dispose();
        }

        _fileWatchers.Clear();
        _scanTokenSource.Dispose();
    }
}
