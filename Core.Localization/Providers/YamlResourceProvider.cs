using Core.Localization.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Core.Localization.Providers;

/// <summary>
/// Provides resources from YAML files with feature-based localization and async support
/// </summary>
public class YamlResourceProvider : ResourceProviderBase, IDisposable
{
    private readonly LocalizationOptions _options;
    private readonly ILogger<YamlResourceProvider> _logger;
    private readonly ConcurrentDictionary<string, Dictionary<string, object>> _resourceCache;
    private readonly List<FileSystemWatcher> _fileWatchers = new();
    private readonly IDeserializer _yamlDeserializer;
    private readonly CancellationTokenSource _scanTokenSource = new();
    private readonly Timer? _resourceScanTimer;
    private readonly ConcurrentDictionary<string, string> _sectionNameCache = new();

    public YamlResourceProvider(
        IOptions<LocalizationOptions> options,
        ILogger<YamlResourceProvider> logger,
        int priority = 150) : base(priority)
    {
        _options = options.Value;
        _logger = logger;
        _resourceCache = new ConcurrentDictionary<string, Dictionary<string, object>>();

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

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

        // Log what we're looking for
        if (_options.EnableDebugLogging)
        {
            _logger.LogDebug("Looking for key: '{Key}', section: '{Section}', culture: '{Culture}', effectiveKey: '{EffectiveKey}'",
                key, section, cultureName, effectiveKey);
        }

        foreach (var cacheEntry in _resourceCache)
        {
            // Try to match the section and culture from the cache key
            // Cache keys are in format: "sectionName.cultureName" or "cultureName" for default section
            var cacheKey = cacheEntry.Key;

            if (IsCacheKeyMatch(cacheKey, section, cultureName))
            {
                if (_options.EnableDebugLogging)
                {
                    _logger.LogDebug("Matched cache key: '{CacheKey}' for section: '{Section}', culture: '{Culture}'",
                        cacheKey, section, cultureName);
                }

                var resources = cacheEntry.Value;

                // First try direct key lookup (flat structure)
                if (resources.TryGetValue(key, out var directValue))
                {
                    if (_options.EnableDebugLogging)
                    {
                        _logger.LogDebug("Found direct key '{Key}' in resources with value: {Value}",
                            key, directValue?.ToString() ?? "null");
                    }

                    return directValue?.ToString();
                }

                // Try nested value lookup (both flat and Messages sub-dictionary)
                if (TryGetNestedValue(resources, effectiveKey, out var value) && value != null)
                {
                    return value.ToString();
                }

                // Try with Messages prefix if not already tried
                if (!effectiveKey.Contains("Messages."))
                {
                    var messagesKey = $"Messages.{effectiveKey}";
                    if (TryGetNestedValue(resources, messagesKey, out value) && value != null)
                    {
                        return value.ToString();
                    }
                }
            }
        }

        // Try parent culture
        var parentCulture = GetParentCulture(culture);
        if (parentCulture != null)
        {
            return await GetStringAsync(key, parentCulture, section, cancellationToken);
        }

        if (_options.EnableDebugLogging)
        {
            _logger.LogDebug("No value found for key: '{Key}', section: '{Section}', culture: '{Culture}'",
                key, section, cultureName);
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
        foreach (var extension in _options.ResourceFileExtensions)
        {
            var files = Directory.GetFiles(directory, $"*.{extension}", SearchOption.TopDirectoryOnly);

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
    }

    private async Task LoadResourceFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(filePath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath).TrimStart('.');

            // Parse file name to extract section and culture
            // Format expected: section.culture.extension (e.g., users.en.yaml)
            var parts = fileNameWithoutExt.Split('.');

            if (parts.Length < 2)
            {
                _logger.LogWarning("Invalid resource file name format: {FileName}. Expected format: section.culture.extension", fileName);
                return;
            }

            var section = parts[0];
            var culture = parts[1];
            var cacheKey = $"{section}.{culture}";

            // Read and parse the YAML/JSON file
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);

            _logger.LogDebug("Loaded file content for {FilePath}: {Content}", filePath, content);

            var resources = _yamlDeserializer.Deserialize<Dictionary<string, object>>(content);

            if (resources == null)
            {
                _logger.LogWarning("Failed to parse resource file {File}", filePath);
                return;
            }

            // Check if the file contains a section name definition
            if (resources.TryGetValue(_options.SectionKey, out var sectionNameObj) &&
                sectionNameObj is string sectionName)
            {
                _sectionNameCache[section] = sectionName;
                _logger.LogDebug("Found section name '{SectionName}' for section '{Section}'", sectionName, section);
            }

            // Store in cache
            _resourceCache[cacheKey] = resources;

            _logger.LogInformation("Loaded resource file {File} with section {Section} and culture {Culture}",
                filePath, section, culture);

            // Log all keys found in this file
            if (_options.EnableDebugLogging)
            {
                var allKeys = GetAllKeysFromDictionary(resources);
                _logger.LogDebug("Keys found in {File}: {Keys}", filePath, string.Join(", ", allKeys));
            }
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
                    foreach (var extension in _options.ResourceFileExtensions)
                    {
                        var watcher = new FileSystemWatcher(localesDir, $"*.{extension}")
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

                        _logger.LogInformation("Initialized file watcher for {Directory} with pattern *.{Extension}",
                            localesDir, extension);
                    }
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

    protected virtual bool TryGetNestedValue(Dictionary<string, object> resources, string key, out object? value)
    {
        value = null;

        // First, try direct key lookup (flat structure)
        if (resources.TryGetValue(key, out var directValue))
        {
            value = directValue;
            if (_options.EnableDebugLogging)
            {
                _logger.LogDebug("Found direct key: {Key} => {Value}", key, directValue);
            }
            return true;
        }

        // Next, try "Messages.key" format if not already prefixed
        if (!key.StartsWith("Messages.", StringComparison.OrdinalIgnoreCase))
        {
            var keyWithoutPrefix = key;

            if (resources.TryGetValue("Messages", out var messagesObj))
            {
                if (messagesObj is Dictionary<object, object> messagesDict)
                {
                    var convertedDict = messagesDict.ToDictionary(kvp => kvp.Key.ToString()!, kvp => kvp.Value);

                    if (convertedDict.TryGetValue(keyWithoutPrefix, out var nestedValue))
                    {
                        value = nestedValue;
                        if (_options.EnableDebugLogging)
                        {
                            _logger.LogDebug("Found in Messages dictionary: {Key} => {Value}", keyWithoutPrefix, nestedValue);
                        }
                        return true;
                    }
                }
                else if (messagesObj is IDictionary<string, object> typedMessagesDict)
                {
                    if (typedMessagesDict.TryGetValue(keyWithoutPrefix, out var nestedValue))
                    {
                        value = nestedValue;
                        if (_options.EnableDebugLogging)
                        {
                            _logger.LogDebug("Found in typed Messages dictionary: {Key} => {Value}", keyWithoutPrefix, nestedValue);
                        }
                        return true;
                    }
                }
            }
        }

        // Finally, fall back to the original nested traversal
        var keys = key.Split('.');

        // Starting point
        Dictionary<string, object>? current = resources;

        try
        {
            // Navigate through nested levels
            for (int i = 0; i < keys.Length - 1; i++)
            {
                var k = keys[i];

                if (!current!.TryGetValue(k, out var nextValue))
                {
                    if (_options.EnableDebugLogging)
                    {
                        _logger.LogDebug("Key not found in path: {Key}", k);
                    }
                    return false;
                }

                if (nextValue is Dictionary<object, object> dict)
                {
                    // Convert from Dictionary<object, object> to Dictionary<string, object>
                    current = dict.ToDictionary(kvp => kvp.Key.ToString()!, kvp => kvp.Value);
                }
                else if (nextValue is IDictionary<string, object> typedDict)
                {
                    current = typedDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                else
                {
                    if (_options.EnableDebugLogging)
                    {
                        _logger.LogDebug("Value is not a dictionary: {Value}", nextValue);
                    }
                    return false;
                }
            }

            // Get the final value
            var lastKey = keys.Last();
            if (current!.TryGetValue(lastKey, out var finalValue))
            {
                value = finalValue;
                if (_options.EnableDebugLogging)
                {
                    _logger.LogDebug("Found through path traversal: {Path} => {Value}", key, finalValue);
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            if (_options.EnableDebugLogging)
            {
                _logger.LogDebug("Error in path traversal: {Error}", ex.Message);
            }
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

            if (kvp.Value is Dictionary<object, object> dict)
            {
                // Convert from Dictionary<object, object> to Dictionary<string, object>
                var convertedDict = dict.ToDictionary(d => d.Key.ToString()!, d => d.Value);
                keys.AddRange(GetAllKeysFromDictionary(convertedDict, fullKey));
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