using Core.Localization.Abstractions;
using Core.Localization.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Core.Localization.Services;

/// <summary>
/// Main localization service implementation with async support and feature-based localization
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IEnumerable<IResourceProvider> _resourceProviders;
    private readonly LocalizationOptions _options;
    private readonly ILogger<LocalizationService> _logger;
    private readonly IMemoryCache? _cache;

    /// <summary>
    /// Creates a new localization service
    /// </summary>
    public LocalizationService(
        IEnumerable<IResourceProvider> resourceProviders,
        IOptions<LocalizationOptions> options,
        ILogger<LocalizationService> logger,
        IMemoryCache? cache = null)
    {
        _resourceProviders = resourceProviders.OrderByDescending(p => p.Priority);
        _options = options.Value;
        _logger = logger;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<string> GetStringAsync(string key, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= _options.DefaultCulture;
        var cacheKey = GetCacheKey(key, culture);

        if (_options.EnableCaching && _cache != null && _cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue ?? key;
        }

        var value = await GetStringFromProvidersAsync(key, culture, null, cancellationToken);

        if (value == null && _options.UseFallbackCulture && !culture.Equals(_options.FallbackCulture))
        {
            value = await GetStringFromProvidersAsync(key, _options.FallbackCulture, null, cancellationToken);
        }

        if (value == null)
        {
            if (_options.ThrowOnMissingResource)
            {
                throw new KeyNotFoundException($"Resource key '{key}' not found for culture '{culture.Name}'");
            }

            _logger.LogWarning("Resource key '{Key}' not found for culture '{Culture}'", key, culture.Name);
            return key;
        }

        if (_options.EnableCaching && _cache != null)
        {
            _cache.Set(cacheKey, value, _options.CacheExpiration);
        }

        return value;
    }

    /// <inheritdoc/>
    public async Task<string> GetStringAsync(string key, string section, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= _options.DefaultCulture;
        var cacheKey = GetCacheKey(key, culture, section);

        if (_options.EnableCaching && _cache != null && _cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue ?? key;
        }

        var value = await GetStringFromProvidersAsync(key, culture, section, cancellationToken);

        if (value == null && _options.UseFallbackCulture && !culture.Equals(_options.FallbackCulture))
        {
            value = await GetStringFromProvidersAsync(key, _options.FallbackCulture, section, cancellationToken);
        }

        if (value == null)
        {
            if (_options.ThrowOnMissingResource)
            {
                throw new KeyNotFoundException($"Resource key '{key}' not found in section '{section}' for culture '{culture.Name}'");
            }

            _logger.LogWarning("Resource key '{Key}' not found in section '{Section}' for culture '{Culture}'", key, section, culture.Name);
            return key;
        }

        if (_options.EnableCaching && _cache != null)
        {
            _cache.Set(cacheKey, value, _options.CacheExpiration);
        }

        return value;
    }

    /// <inheritdoc/>
    public async Task<string> GetStringAsync(string key, params object[] args)
    {
        return await GetStringAsync(key, _options.DefaultCulture, args);
    }

    /// <inheritdoc/>
    public async Task<string> GetStringAsync(string key, CultureInfo culture, params object[] args)
    {
        var format = await GetStringAsync(key, culture);

        try
        {
            return string.Format(culture, format, args);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Error formatting string '{Key}' with culture '{Culture}'", key, culture.Name);
            return format;
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetStringAsync(string key, string section, params object[] args)
    {
        return await GetStringAsync(key, section, _options.DefaultCulture, args);
    }

    /// <inheritdoc/>
    public async Task<string> GetStringAsync(string key, string section, CultureInfo culture, params object[] args)
    {
        var format = await GetStringAsync(key, section, culture);

        try
        {
            return string.Format(culture, format, args);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Error formatting string '{Key}' with section '{Section}' and culture '{Culture}'", key, section, culture.Name);
            return format;
        }
    }

    /// <inheritdoc/>
    public async Task<LocalizationResult> TryGetStringAsync(string key, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= _options.DefaultCulture;
        var value = await GetStringFromProvidersAsync(key, culture, null, cancellationToken);

        if (value == null && _options.UseFallbackCulture && !culture.Equals(_options.FallbackCulture))
        {
            value = await GetStringFromProvidersAsync(key, _options.FallbackCulture, null, cancellationToken);
        }

        return value != null
            ? LocalizationResult.Successful(value)
            : LocalizationResult.Failed();
    }

    /// <inheritdoc/>
    public async Task<LocalizationResult> TryGetStringAsync(string key, string section, CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= _options.DefaultCulture;
        var value = await GetStringFromProvidersAsync(key, culture, section, cancellationToken);

        if (value == null && _options.UseFallbackCulture && !culture.Equals(_options.FallbackCulture))
        {
            value = await GetStringFromProvidersAsync(key, _options.FallbackCulture, section, cancellationToken);
        }

        return value != null
            ? LocalizationResult.Successful(value)
            : LocalizationResult.Failed();
    }

    /// <inheritdoc/>
    public async Task<IDictionary<CultureInfo, string>> GetAllStringsAsync(string key, string? section = null, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<CultureInfo, string>();
        var cultures = await GetSupportedCulturesAsync(cancellationToken);

        foreach (var culture in cultures)
        {
            var tryResult = await TryGetStringAsync(key, section, culture, cancellationToken);

            if (tryResult.Success && tryResult.Value != null)
            {
                result[culture] = tryResult.Value;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetAllKeysAsync(string? section = null, CancellationToken cancellationToken = default)
    {
        return await GetAllKeysAsync(_options.DefaultCulture, section, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetAllKeysAsync(CultureInfo culture, string? section = null, CancellationToken cancellationToken = default)
    {
        var keys = new HashSet<string>();

        foreach (var provider in _resourceProviders)
        {
            var providerKeys = await provider.GetAllKeysAsync(culture, section, cancellationToken);
            foreach (var key in providerKeys)
            {
                keys.Add(key);
            }
        }

        return keys;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetAllSectionsAsync(CultureInfo? culture = null, CancellationToken cancellationToken = default)
    {
        culture ??= _options.DefaultCulture;
        var sections = new HashSet<string>();

        foreach (var provider in _resourceProviders)
        {
            var providerSections = await provider.GetAllSectionsAsync(culture, cancellationToken);
            foreach (var section in providerSections)
            {
                sections.Add(section);
            }
        }

        return sections;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CultureInfo>> GetSupportedCulturesAsync(CancellationToken cancellationToken = default)
    {
        return _options.SupportedCultures;
    }

    private async Task<string?> GetStringFromProvidersAsync(string key, CultureInfo culture, string? section, CancellationToken cancellationToken)
    {
        foreach (var provider in _resourceProviders)
        {
            var value = await provider.GetStringAsync(key, culture, section, cancellationToken);
            if (value != null)
            {
                return value;
            }
        }

        return null;
    }

    private string GetCacheKey(string key, CultureInfo culture, string? section = null)
    {
        return string.IsNullOrEmpty(section)
            ? $"{culture.Name}:{key}"
            : $"{culture.Name}:{section}:{key}";
    }
}
