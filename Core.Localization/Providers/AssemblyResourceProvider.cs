using Core.Localization.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Core.Localization.Providers;

/// <summary>
/// Provides resources from YAML/JSON files located in specified assemblies with feature-based localization and async support
/// </summary>
public class AssemblyResourceProvider : ResourceProviderBase, IDisposable
{
    private readonly LocalizationOptions _options;
    private readonly ILogger<AssemblyResourceProvider> _logger;
    private readonly ConcurrentDictionary<string, Dictionary<string, object>> _resourceCache;
    private readonly IDeserializer _yamlDeserializer;
    private readonly ConcurrentDictionary<string, string> _sectionNameCache = new();
    private readonly ICollection<Assembly> _assemblies;

    public AssemblyResourceProvider(
        IOptions<LocalizationOptions> options,
        ILogger<AssemblyResourceProvider> logger,
        IEnumerable<Assembly> assemblies,
        int priority = 400) : base(priority)
    {
        _options = options.Value;
        _logger = logger;
        _resourceCache = new ConcurrentDictionary<string, Dictionary<string, object>>();
        _assemblies = assemblies.ToList();

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        // Load resources initially
        LoadResourcesAsync().GetAwaiter().GetResult();
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

        // Scan for resources in all registered assemblies
        foreach (var assembly in _assemblies)
        {
            await ScanAssemblyForResourcesAsync(assembly, cancellationToken);
        }
    }

    private async Task ScanAssemblyForResourcesAsync(Assembly assembly, CancellationToken cancellationToken)
    {
        try
        {
            // Get the assembly location
            var assemblyLocation = assembly.Location;
            if (string.IsNullOrEmpty(assemblyLocation))
            {
                _logger.LogWarning("Assembly {Assembly} location is null or empty", assembly.FullName);
                return;
            }

            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            if (string.IsNullOrEmpty(assemblyDirectory))
            {
                _logger.LogWarning("Assembly {Assembly} directory is null or empty", assembly.FullName);
                return;
            }

            _logger.LogInformation("Scanning assembly {Assembly} at {Location} for resources", assembly.GetName().Name, assemblyDirectory);

            // Find all feature directories
            foreach (var basePath in _options.ResourcePaths)
            {
                var fullBasePath = Path.Combine(assemblyDirectory, basePath);

                if (!Directory.Exists(fullBasePath))
                {
                    _logger.LogWarning("Resource path {Path} does not exist in assembly {Assembly}", fullBasePath, assembly.GetName().Name);
                    continue;
                }

                // Find feature directories
                var featureDirectories = Directory.GetDirectories(fullBasePath);

                foreach (var featureDir in featureDirectories)
                {
                    var featureName = Path.GetFileName(featureDir);
                    var localesDir = Path.Combine(featureDir, "Resources", "Locales");

                    if (Directory.Exists(localesDir))
                    {
                        await LoadResourceFilesFromDirectoryAsync(localesDir, featureName, cancellationToken);
                    }
                    else
                    {
                        // Try alternative patterns like the root Resources/Locales folder
                        foreach (var pattern in _options.FeatureDirectoryPattern.Split('|'))
                        {
                            // Replace ** with recursion
                            if (pattern.Contains("**"))
                            {
                                // Find all matching directories recursively
                                var allDirs = Directory.GetDirectories(featureDir, "*", SearchOption.AllDirectories);
                                foreach (var dir in allDirs)
                                {
                                    if (dir.EndsWith("Resources\\Locales") || dir.EndsWith("Resources/Locales"))
                                    {
                                        await LoadResourceFilesFromDirectoryAsync(dir, featureName, cancellationToken);
                                    }
                                }
                            }
                            else
                            {
                                // Direct path match
                                var dirPath = Path.Combine(featureDir, pattern.Replace("Resources/Locales", "Resources\\Locales"));
                                if (Directory.Exists(dirPath))
                                {
                                    await LoadResourceFilesFromDirectoryAsync(dirPath, featureName, cancellationToken);
                                }
                            }
                        }
                    }
                }

                // Also check for base Locales directory
                var baseLocalesPath = Path.Combine(fullBasePath, "Resources", "Locales");
                if (Directory.Exists(baseLocalesPath))
                {
                    await LoadResourceFilesFromDirectoryAsync(baseLocalesPath, "Common", cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning assembly {Assembly} for resources", assembly.FullName);
        }
    }

    private async Task LoadResourceFilesFromDirectoryAsync(string directory, string defaultSection, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loading resource files from directory {Directory} with default section {Section}", directory, defaultSection);

        foreach (var extension in _options.ResourceFileExtensions)
        {
            var files = Directory.GetFiles(directory, $"*.{extension}", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                try
                {
                    await LoadResourceFileAsync(file, defaultSection, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load resource file {File}", file);
                }
            }
        }
    }

    private async Task LoadResourceFileAsync(string filePath, string defaultSection, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = Path.GetFileName(filePath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath).TrimStart('.');

            _logger.LogInformation("Loading resource file {File}", filePath);

            // Parse file name to extract section and culture
            // Format expected: section.culture.extension (e.g., users.en.yaml)
            var parts = fileNameWithoutExt.Split('.');

            if (parts.Length < 2)
            {
                _logger.LogWarning("Invalid resource file name format: {FileName}. Expected format: section.culture.extension", fileName);
                return;
            }

            var section = parts.Length >= 2 ? parts[0] : defaultSection;
            var culture = parts.Length >= 2 ? parts[1] : parts[0];
            var cacheKey = $"{section}.{culture}";

            // Read file content
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);

            // Parse based on file extension
            Dictionary<string, object> resources;

            if (extension.Equals("yaml", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals("yml", StringComparison.OrdinalIgnoreCase))
            {
                resources = _yamlDeserializer.Deserialize<Dictionary<string, object>>(content);
            }
            else if (extension.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                // Parse JSON
                resources = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(content,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new Dictionary<string, object>();
            }
            else
            {
                _logger.LogWarning("Unsupported file extension: {Extension} for file {File}", extension, filePath);
                return;
            }

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
        // Nothing to dispose
    }
}
