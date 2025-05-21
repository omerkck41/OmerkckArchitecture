using Core.Localization.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Core.Localization.Providers;

/// <summary>
/// Provides resources from YAML/JSON files embedded in assemblies with feature-based localization and async support
/// </summary>
public sealed class AssemblyResourceProvider : ResourceProviderBase, IDisposable
{
    private readonly LocalizationOptions _options;
    private readonly ILogger<AssemblyResourceProvider> _logger;
    private readonly Dictionary<string, Dictionary<string, object>> _resourceCache = new();
    private readonly IDeserializer _yamlDeserializer;
    private readonly Dictionary<string, string> _sectionNameCache = new();
    private readonly ICollection<Assembly> _assemblies;

    /// <summary>
    /// Creates a new assembly resource provider that loads resources from specified assemblies
    /// </summary>
    /// <param name="options">Localization options</param>
    /// <param name="logger">Logger</param>
    /// <param name="assemblies">Assemblies to load resources from</param>
    /// <param name="priority">Provider priority</param>
    public AssemblyResourceProvider(
        IOptions<LocalizationOptions> options,
        ILogger<AssemblyResourceProvider> logger,
        IEnumerable<Assembly> assemblies,
        int priority = 400) : base(priority)
    {
        _options = options.Value;
        _logger = logger;
        _assemblies = assemblies.ToList();

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        // Load resources initially
        LoadResourcesAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public override bool SupportsDynamicReload => true;

    /// <inheritdoc/>
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

                // Try direct key lookup (flat structure)
                if (resources.TryGetValue(key, out var directValue))
                {
                    if (_options.EnableDebugLogging)
                    {
                        _logger.LogDebug("Found direct key '{Key}' in resources with value: {Value}",
                            key, directValue?.ToString() ?? "null");
                    }

                    return directValue?.ToString();
                }

                // Try nested value lookup
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

        if (_options.EnableDebugLogging)
        {
            _logger.LogDebug("No value found for key: '{Key}', section: '{Section}', culture: '{Culture}'",
                key, section, cultureName);
        }

        return null;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
            _logger.LogInformation("Scanning assembly {Assembly} for resources", assembly.GetName().Name);

            // Get all embedded resource names
            var resourceNames = assembly.GetManifestResourceNames();

            // Filter for localization resources
            foreach (var resourceName in resourceNames)
            {
                try
                {
                    if (IsLocalizationResource(resourceName))
                    {
                        await ProcessEmbeddedResourceAsync(assembly, resourceName, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing embedded resource {ResourceName}", resourceName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning assembly {Assembly} for resources", assembly.FullName);
        }
    }

    private bool IsLocalizationResource(string resourceName)
    {
        // Check if the resource is in a Resources/Locales directory
        return resourceName.Contains("Resources.Locales") &&
               (_options.ResourceFileExtensions.Any(ext => resourceName.EndsWith($".{ext}", StringComparison.OrdinalIgnoreCase)));
    }

    private async Task ProcessEmbeddedResourceAsync(Assembly assembly, string resourceName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing embedded resource {ResourceName}", resourceName);

        // Extract section and culture from resource name
        // Format expected: Project.Features.SectionName.Resources.Locales.section.culture.extension
        var nameParts = resourceName.Split('.');

        if (nameParts.Length < 4)
        {
            _logger.LogWarning("Invalid resource name format: {ResourceName}", resourceName);
            return;
        }

        // The last three parts should be: section.culture.extension
        var extension = nameParts[^1].ToLowerInvariant();
        var culture = nameParts[^2];
        var section = nameParts[^3];

        var cacheKey = $"{section}.{culture}";

        try
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                _logger.LogWarning("Resource stream is null for {ResourceName}", resourceName);
                return;
            }

            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync(cancellationToken);

            Dictionary<string, object> resources;

            // Parse based on file extension
            if (extension == "yaml" || extension == "yml")
            {
                resources = _yamlDeserializer.Deserialize<Dictionary<string, object>>(content);
            }
            else if (extension == "json")
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
                _logger.LogWarning("Unsupported file extension: {Extension} for resource {ResourceName}", extension, resourceName);
                return;
            }

            if (resources == null)
            {
                _logger.LogWarning("Failed to parse resource {ResourceName}", resourceName);
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

            _logger.LogInformation("Loaded embedded resource {ResourceName} with section {Section} and culture {Culture}",
                resourceName, section, culture);

            // Log all keys found in this resource
            if (_options.EnableDebugLogging)
            {
                var allKeys = GetAllKeysFromDictionary(resources);
                _logger.LogDebug("Keys found in {ResourceName}: {Keys}", resourceName, string.Join(", ", allKeys));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading embedded resource {ResourceName}", resourceName);
        }
    }

    private bool TryGetNestedValue(Dictionary<string, object> resources, string key, out object? value)
    {
        value = null;
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

    /// <summary>
    /// Disposes resources used by this provider
    /// </summary>
    public void Dispose()
    {
        // Nothing to dispose
    }
}
