using Core.Localization.Abstractions;
using System.Globalization;

namespace Core.Localization.Providers;

/// <summary>
/// Base class for resource providers with async support
/// </summary>
public abstract class ResourceProviderBase : IResourceProvider
{
    protected ResourceProviderBase(int priority = 100)
    {
        Priority = priority;
    }

    public virtual int Priority { get; }

    public virtual bool SupportsDynamicReload => false;

    public abstract Task<string?> GetStringAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default);

    public virtual async Task<object?> GetResourceAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        return await GetStringAsync(key, culture, section, cancellationToken);
    }

    public abstract Task<IEnumerable<string>> GetAllKeysAsync(CultureInfo culture, string? section = null, CancellationToken cancellationToken = default);

    public abstract Task<IEnumerable<string>> GetAllSectionsAsync(CultureInfo culture, CancellationToken cancellationToken = default);

    public virtual async Task<bool> HasKeyAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        return (await GetStringAsync(key, culture, section, cancellationToken)) != null;
    }

    public virtual Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        if (!SupportsDynamicReload)
        {
            throw new NotSupportedException($"Provider {GetType().Name} does not support dynamic reloading.");
        }

        return Task.CompletedTask;
    }

    protected virtual CultureInfo? GetParentCulture(CultureInfo culture)
    {
        if (culture.Parent == CultureInfo.InvariantCulture)
        {
            return null;
        }

        return culture.Parent;
    }

    /// <summary>
    /// Gets the effective section name by combining section with key if necessary
    /// </summary>
    /// <param name="key">Resource key</param>
    /// <param name="section">Optional section name</param>
    /// <returns>Effective key to lookup in resources</returns>
    protected virtual string GetEffectiveKey(string key, string? section)
    {
        if (string.IsNullOrEmpty(section))
            return key;

        // Check if the key already contains the section name
        if (key.StartsWith($"{section}.", StringComparison.OrdinalIgnoreCase))
            return key;

        return $"{section}.{key}";
    }

    /// <summary>
    /// Gets the normalized culture code based on the culture info
    /// </summary>
    /// <param name="culture">Culture info</param>
    /// <returns>Normalized culture code (e.g., "en" from "en-US")</returns>
    protected virtual string GetNormalizedCultureCode(CultureInfo culture)
    {
        // For YAML files, we typically use language code only (e.g., "en" instead of "en-US")
        return culture.TwoLetterISOLanguageName.ToLowerInvariant();
    }
}
