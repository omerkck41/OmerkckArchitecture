using System.Globalization;
using Kck.Localization.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kck.Localization;

/// <summary>
/// Orchestrates multiple resource providers with priority, fallback, and pluralization support.
/// </summary>
public sealed class LocalizationService(
    IEnumerable<IResourceProvider> providers,
    IPluralizer pluralizer,
    IOptionsMonitor<LocalizationOptions> options,
    ILogger<LocalizationService> logger) : ILocalizationService
{
    private readonly IReadOnlyList<IResourceProvider> _providers =
        providers.OrderBy(p => p.Priority).ToList().AsReadOnly();
    private readonly IReadOnlyList<IResourceProvider> _reversedProviders =
        providers.OrderByDescending(p => p.Priority).ToList().AsReadOnly();
    private readonly LocalizationOptions _options = options.CurrentValue;

    public Task<string> GetStringAsync(string key, CancellationToken ct = default) =>
        GetStringAsync(key, _options.DefaultCulture, ct);

    public Task<string> GetStringAsync(string key, string culture, CancellationToken ct = default) =>
        GetStringAsync(key, culture, [], ct);

    public async Task<string> GetStringAsync(string key, string culture, object[] args, CancellationToken ct = default)
    {
        var value = await ResolveStringAsync(key, culture, ct).ConfigureAwait(false);

        if (value is null)
        {
            if (_options.ThrowOnMissing)
                throw new KeyNotFoundException($"Localization key '{key}' not found for culture '{culture}'.");

            logger.LogDebug("Missing localization key: {Key} for culture: {Culture}", key, culture);
            return string.Format(_options.MissingKeyPattern, key);
        }

        return args.Length > 0 ? string.Format(value, args) : value;
    }

    public async Task<string?> TryGetStringAsync(string key, string culture, CancellationToken ct = default) =>
        await ResolveStringAsync(key, culture, ct).ConfigureAwait(false);

    public async Task<string> GetPluralStringAsync(string key, int count, string culture, CancellationToken ct = default)
    {
        var category = pluralizer.GetPluralCategory(count, culture);
        var pluralKey = $"{key}.{category}";

        var value = await ResolveStringAsync(pluralKey, culture, ct).ConfigureAwait(false);

        // Fall back to "other" if the specific category is missing
        if (value is null && category != "other")
        {
            var otherKey = $"{key}.other";
            value = await ResolveStringAsync(otherKey, culture, ct).ConfigureAwait(false);
        }

        if (value is null)
        {
            if (_options.ThrowOnMissing)
                throw new KeyNotFoundException($"Plural key '{pluralKey}' not found for culture '{culture}'.");

            return string.Format(_options.MissingKeyPattern, pluralKey);
        }

        return string.Format(value, count);
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAllStringsAsync(string culture, CancellationToken ct = default)
    {
        var merged = new Dictionary<string, string>();

        // Iterate in reverse priority order so higher-priority providers overwrite
        foreach (var provider in _reversedProviders)
        {
            var strings = await provider.GetAllStringsAsync(culture, ct).ConfigureAwait(false);
            foreach (var kvp in strings)
                merged[kvp.Key] = kvp.Value;
        }

        return merged.AsReadOnly();
    }

    public async Task<IReadOnlyList<string>> GetAllKeysAsync(string culture, CancellationToken ct = default)
    {
        var keys = new HashSet<string>();

        foreach (var provider in _providers)
        {
            var strings = await provider.GetAllStringsAsync(culture, ct).ConfigureAwait(false);
            foreach (var key in strings.Keys)
                keys.Add(key);
        }

        return keys.ToList().AsReadOnly();
    }

    public async Task ReloadAsync(CancellationToken ct = default)
    {
        foreach (var provider in _providers.Where(p => p.SupportsDynamicReload))
        {
            await provider.ReloadAsync(ct).ConfigureAwait(false);
        }
    }

    private async Task<string?> ResolveStringAsync(string key, string culture, CancellationToken ct)
    {
        // 1. Try exact culture across all providers
        var value = await QueryProvidersAsync(key, culture, ct).ConfigureAwait(false);
        if (value is not null)
            return value;

        // 2. Try parent culture (e.g., "en-US" -> "en")
        var parentCulture = GetParentCulture(culture);
        if (parentCulture is not null && parentCulture != culture)
        {
            value = await QueryProvidersAsync(key, parentCulture, ct).ConfigureAwait(false);
            if (value is not null)
                return value;
        }

        // 3. Try fallback culture
        if (culture != _options.FallbackCulture)
        {
            value = await QueryProvidersAsync(key, _options.FallbackCulture, ct).ConfigureAwait(false);
            if (value is not null)
                return value;
        }

        return null;
    }

    private async Task<string?> QueryProvidersAsync(string key, string culture, CancellationToken ct)
    {
        foreach (var provider in _providers)
        {
            var value = await provider.GetStringAsync(key, culture, ct).ConfigureAwait(false);
            if (value is not null)
                return value;
        }

        return null;
    }

    private static string? GetParentCulture(string culture)
    {
        try
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            var parent = cultureInfo.Parent;
            return parent != CultureInfo.InvariantCulture ? parent.Name : null;
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }
}
