using System.Globalization;

namespace Core.Localization.Providers;

/// <summary>
/// In-memory resource provider for testing and simple scenarios with async support
/// </summary>
public class InMemoryResourceProvider : ResourceProviderBase
{
    private readonly IDictionary<string, IDictionary<string, string>> _resources;
    private readonly IDictionary<string, string> _sectionMappings;

    /// <summary>
    /// Creates a new in-memory resource provider with the specified resources
    /// </summary>
    /// <param name="resources">The resources to use</param>
    /// <param name="priority">The provider priority</param>
    public InMemoryResourceProvider(
        IDictionary<string, IDictionary<string, string>> resources,
        int priority = 300) : base(priority)
    {
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        _sectionMappings = new Dictionary<string, string>();
    }

    /// <inheritdoc/>
    public override Task<string?> GetStringAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        var cultureName = culture.Name;
        var effectiveKey = GetEffectiveKey(key, section);

        if (_resources.TryGetValue(cultureName, out var cultureResources) &&
            cultureResources.TryGetValue(effectiveKey, out var value))
        {
            return Task.FromResult<string?>(value);
        }

        // Try neutral culture
        var neutralCulture = culture.Parent;
        if (neutralCulture != CultureInfo.InvariantCulture &&
            _resources.TryGetValue(neutralCulture.Name, out cultureResources) &&
            cultureResources.TryGetValue(effectiveKey, out value))
        {
            return Task.FromResult<string?>(value);
        }

        // Try invariant culture
        if (_resources.TryGetValue("", out cultureResources) &&
            cultureResources.TryGetValue(effectiveKey, out value))
        {
            return Task.FromResult<string?>(value);
        }

        return Task.FromResult<string?>(null);
    }

    /// <inheritdoc/>
    public override Task<IEnumerable<string>> GetAllKeysAsync(CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        var cultureName = culture.Name;
        var prefix = section != null ? $"{section}." : "";

        if (_resources.TryGetValue(cultureName, out var cultureResources))
        {
            var keys = cultureResources.Keys
                .Where(k => string.IsNullOrEmpty(section) || k.StartsWith(prefix))
                .Select(k => string.IsNullOrEmpty(section) ? k : k[prefix.Length..])
                .ToList();

            return Task.FromResult<IEnumerable<string>>(keys);
        }

        return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
    }

    /// <inheritdoc/>
    public override Task<IEnumerable<string>> GetAllSectionsAsync(CultureInfo culture, CancellationToken cancellationToken = default)
    {
        var cultureName = culture.Name;
        var sections = new HashSet<string>();

        if (_resources.TryGetValue(cultureName, out var cultureResources))
        {
            foreach (var key in cultureResources.Keys)
            {
                var dotIndex = key.IndexOf('.');
                if (dotIndex > 0)
                {
                    var section = key[..dotIndex];
                    sections.Add(section);
                }
            }
        }

        // Add sections from mappings
        foreach (var section in _sectionMappings.Values)
        {
            sections.Add(section);
        }

        return Task.FromResult<IEnumerable<string>>(sections);
    }

    /// <summary>
    /// Adds or updates a resource for a specific culture
    /// </summary>
    /// <param name="cultureName">The culture name</param>
    /// <param name="key">The resource key</param>
    /// <param name="value">The resource value</param>
    /// <param name="section">Optional section name</param>
    public void AddOrUpdateResource(string cultureName, string key, string value, string? section = null)
    {
        if (!_resources.TryGetValue(cultureName, out var cultureResources))
        {
            cultureResources = new Dictionary<string, string>();
            _resources[cultureName] = cultureResources;
        }

        var effectiveKey = GetEffectiveKey(key, section);
        cultureResources[effectiveKey] = value;
    }

    /// <summary>
    /// Adds a section name mapping
    /// </summary>
    /// <param name="sectionKey">The section key</param>
    /// <param name="sectionName">The section name</param>
    public void AddSectionMapping(string sectionKey, string sectionName)
    {
        _sectionMappings[sectionKey] = sectionName;
    }

    /// <summary>
    /// Removes a resource for a specific culture
    /// </summary>
    /// <param name="cultureName">The culture name</param>
    /// <param name="key">The resource key</param>
    /// <param name="section">Optional section name</param>
    /// <returns>True if the resource was removed, false otherwise</returns>
    public bool RemoveResource(string cultureName, string key, string? section = null)
    {
        if (_resources.TryGetValue(cultureName, out var cultureResources))
        {
            var effectiveKey = GetEffectiveKey(key, section);
            return cultureResources.Remove(effectiveKey);
        }

        return false;
    }

    /// <summary>
    /// Clears all resources
    /// </summary>
    public void Clear()
    {
        _resources.Clear();
        _sectionMappings.Clear();
    }

    /// <summary>
    /// Gets the number of resources
    /// </summary>
    /// <returns>The number of resources</returns>
    public int GetResourceCount()
    {
        return _resources.Sum(cr => cr.Value.Count);
    }

    /// <summary>
    /// Gets all resources for a specific culture
    /// </summary>
    /// <param name="cultureName">The culture name</param>
    /// <returns>Dictionary of resources for the specified culture</returns>
    public IDictionary<string, string>? GetResourcesForCulture(string cultureName)
    {
        return _resources.TryGetValue(cultureName, out var cultureResources)
            ? cultureResources
            : null;
    }
}
