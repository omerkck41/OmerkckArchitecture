﻿using Core.Localization.Abstractions;
using Core.Localization.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace Core.Localization.Services;

/// <summary>
/// Main localization service implementation
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly IEnumerable<IResourceProvider> _resourceProviders;
    private readonly LocalizationOptions _options;
    private readonly ILogger<LocalizationService> _logger;
    private readonly IMemoryCache? _cache;

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

    public string GetString(string key, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        var cacheKey = GetCacheKey(key, culture);

        if (_options.EnableCaching && _cache != null && _cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue ?? key;
        }

        var value = GetStringFromProviders(key, culture);

        if (value == null && _options.UseFallbackCulture && !culture.Equals(_options.FallbackCulture))
        {
            value = GetStringFromProviders(key, _options.FallbackCulture);
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

    public string GetString(string key, params object[] args)
    {
        return GetString(key, CultureInfo.CurrentCulture, args);
    }

    public string GetString(string key, CultureInfo culture, params object[] args)
    {
        var format = GetString(key, culture);

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

    public bool TryGetString(string key, out string? value, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        value = GetStringFromProviders(key, culture);

        if (value == null && _options.UseFallbackCulture && !culture.Equals(_options.FallbackCulture))
        {
            value = GetStringFromProviders(key, _options.FallbackCulture);
        }

        return value != null;
    }

    public IDictionary<CultureInfo, string> GetAllStrings(string key)
    {
        var result = new Dictionary<CultureInfo, string>();

        foreach (var culture in _options.SupportedCultures)
        {
            if (TryGetString(key, out var value, culture) && value != null)
            {
                result[culture] = value;
            }
        }

        return result;
    }

    public IEnumerable<string> GetAllKeys()
    {
        return GetAllKeys(CultureInfo.CurrentCulture);
    }

    public IEnumerable<string> GetAllKeys(CultureInfo culture)
    {
        var keys = new HashSet<string>();

        foreach (var provider in _resourceProviders)
        {
            foreach (var key in provider.GetAllKeys(culture))
            {
                keys.Add(key);
            }
        }

        return keys;
    }

    public IEnumerable<CultureInfo> GetSupportedCultures()
    {
        return _options.SupportedCultures;
    }

    private string? GetStringFromProviders(string key, CultureInfo culture)
    {
        foreach (var provider in _resourceProviders)
        {
            var value = provider.GetString(key, culture);
            if (value != null)
            {
                return value;
            }
        }

        return null;
    }

    private string GetCacheKey(string key, CultureInfo culture)
    {
        return $"{culture.Name}:{key}";
    }
}
