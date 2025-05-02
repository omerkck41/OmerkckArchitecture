using System.Globalization;

namespace Core.Localization.Providers;

/// <summary>
/// In-memory resource provider for testing and simple scenarios with async support
/// </summary>
public class InMemoryResourceProvider : ResourceProviderBase
{
    private readonly IDictionary<string, IDictionary<string, string>> _resources;
    private readonly IDictionary<string, string> _sectionMappings;

    public InMemoryResourceProvider(
        IDictionary<string, IDictionary<string, string>> resources,
        int priority = 300) : base(priority)
    {
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        _sectionMappings = new Dictionary<string, string>();
    }

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

    public override Task<IEnumerable<string>> GetAllKeysAsync(CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        var cultureName = culture.Name;
        var prefix = section != null ? $"{section}." : "";

        if (_resources.TryGetValue(cultureName, out var cultureResources))
        {
            var keys = cultureResources.Keys
                .Where(k => string.IsNullOrEmpty(section) || k.StartsWith(prefix))
                .Select(k => string.IsNullOrEmpty(section) ? k : k.Substring(prefix.Length))
                .ToList();

            return Task.FromResult<IEnumerable<string>>(keys);
        }

        return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
    }

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
                    var section = key.Substring(0, dotIndex);
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
    public void AddSectionMapping(string sectionKey, string sectionName)
    {
        _sectionMappings[sectionKey] = sectionName;
    }

    /// <summary>
    /// Removes a resource for a specific culture
    /// </summary>
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
}
