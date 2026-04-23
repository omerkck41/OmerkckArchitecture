using System.Collections.Concurrent;
using Kck.Caching.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Kck.Caching.InMemory;

public sealed class InMemoryCacheService(
    IMemoryCache cache,
    IOptionsMonitor<CacheOptions> options) : CacheServiceBase
{
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    protected override CacheOptions Options { get; } = options.CurrentValue;

    public override Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : default
    {
        cache.TryGetValue(BuildKey(key), out T? value);
        return Task.FromResult(value);
    }

    public override Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var exp = expiration ?? Options.DefaultExpiration;
        var fullKey = BuildKey(key);
        var entryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(exp)
            .RegisterPostEvictionCallback((evictedKey, _, _, _) =>
                _keys.TryRemove(evictedKey.ToString()!, out _));
        cache.Set(fullKey, value, entryOptions);
        _keys.TryAdd(fullKey, 0);
        return Task.CompletedTask;
    }

    public override Task RemoveAsync(string key, CancellationToken ct = default)
    {
        var fullKey = BuildKey(key);
        cache.Remove(fullKey);
        _keys.TryRemove(fullKey, out _);
        return Task.CompletedTask;
    }

    public override Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        return Task.FromResult(cache.TryGetValue(BuildKey(key), out _));
    }

    public override Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var fullPrefix = BuildKey(prefix);
        var keysToRemove = _keys.Keys
            .Where(k => k.StartsWith(fullPrefix, StringComparison.Ordinal))
            .ToList();

        foreach (var key in keysToRemove)
        {
            cache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
