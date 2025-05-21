using Core.Localization.Abstractions;
using System.Globalization;

namespace Core.Localization.Providers;

/// <summary>
/// Base class for resource providers with async support
/// </summary>
public abstract class ResourceProviderBase : IResourceProvider
{
    /// <summary>
    /// Creates a new instance of the resource provider with the specified priority
    /// </summary>
    /// <param name="priority">Provider priority for conflict resolution</param>
    protected ResourceProviderBase(int priority = 100)
    {
        Priority = priority;
    }

    /// <inheritdoc/>
    public virtual int Priority { get; }

    /// <inheritdoc/>
    public virtual bool SupportsDynamicReload => false;

    /// <inheritdoc/>
    public abstract Task<string?> GetStringAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual async Task<object?> GetResourceAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        return await GetStringAsync(key, culture, section, cancellationToken);
    }

    /// <inheritdoc/>
    public abstract Task<IEnumerable<string>> GetAllKeysAsync(CultureInfo culture, string? section = null, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task<IEnumerable<string>> GetAllSectionsAsync(CultureInfo culture, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual async Task<bool> HasKeyAsync(string key, CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        return (await GetStringAsync(key, culture, section, cancellationToken)) != null;
    }

    /// <inheritdoc/>
    public virtual Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        if (!SupportsDynamicReload)
        {
            throw new NotSupportedException($"Provider {GetType().Name} does not support dynamic reloading.");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the parent culture of the specified culture
    /// </summary>
    /// <param name="culture">The culture to get the parent of</param>
    /// <returns>The parent culture or null if no parent exists</returns>
    protected virtual CultureInfo? GetParentCulture(CultureInfo culture)
    {
        if (culture.Parent == CultureInfo.InvariantCulture)
        {
            return null;
        }

        return culture.Parent;
    }

    /// <summary>
    /// Gets the effective key to lookup in resources by considering different formats:
    /// 1. Direct key: "UserDontExists"
    /// 2. Section.Key: "Users.UserDontExists"
    /// 3. Section.Messages.Key: "Users.Messages.UserDontExists"
    /// </summary>
    /// <param name="key">Resource key</param>
    /// <param name="section">Optional section name</param>
    /// <returns>Effective key to lookup in resources</returns>
    protected virtual string GetEffectiveKey(string key, string? section)
    {
        // If no section, just return the key as is
        if (string.IsNullOrEmpty(section))
            return key;

        // If key already contains section prefix (e.g., "Users.UserDontExists"), return as is
        if (key.StartsWith($"{section}.", StringComparison.OrdinalIgnoreCase))
            return key;

        // If key already has Messages prefix (e.g., "Messages.UserDontExists"), prepend section
        if (key.StartsWith("Messages.", StringComparison.OrdinalIgnoreCase))
            return $"{section}.{key}";

        // If key is a plain key (e.g., "UserDontExists"), combine with section
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
