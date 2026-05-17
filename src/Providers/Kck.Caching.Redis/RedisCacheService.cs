using System.Diagnostics;
using System.Text.Json;
using Kck.Caching.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Kck.Caching.Redis;

/// <summary>
/// Redis-backed implementation of <see cref="CacheServiceBase"/> that serialises values as JSON
/// via <see cref="IDistributedCache"/> and uses <see cref="IConnectionMultiplexer"/> for
/// key-existence checks and prefix-based bulk deletion.
/// </summary>
[DebuggerDisplay("Prefix={Options.KeyPrefix,nq}, Provider=Redis")]
public sealed class RedisCacheService(
    IDistributedCache cache,
    IConnectionMultiplexer redis,
    IOptionsMonitor<CacheOptions> options) : CacheServiceBase
{
    /// <inheritdoc/>
    protected override CacheOptions Options { get; } = options.CurrentValue;

    /// <inheritdoc/>
    public override async ValueTask<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : default
    {
        var data = await cache.GetStringAsync(BuildKey(key), ct).ConfigureAwait(false);
        return data is null ? default : JsonSerializer.Deserialize<T>(data);
    }

    /// <inheritdoc/>
    public override async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var exp = expiration ?? Options.DefaultExpiration;
        var data = JsonSerializer.Serialize(value);
        var entry = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = exp };
        await cache.SetStringAsync(BuildKey(key), data, entry, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await cache.RemoveAsync(BuildKey(key), ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async ValueTask<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        // LS-FAZ-5 (5.6): EXISTS komutu ile sadece varlik kontrolu — payload network'e cikmaz.
        // StackExchange.Redis KeyExistsAsync CancellationToken almıyor (API limiti); mevcut
        // RemoveByPrefixAsync.KeyDeleteAsync ile uyumlu pattern.
        ct.ThrowIfCancellationRequested();
        return await redis.GetDatabase().KeyExistsAsync(BuildKey(key)).ConfigureAwait(false);
    }

    private const int DeleteChunkSize = 1000;

    /// <summary>
    /// Deletes all Redis keys that begin with the given <paramref name="prefix"/> by iterating
    /// every server endpoint and batch-deleting in chunks of <c>1000</c>.
    /// </summary>
    public override async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var fullPrefix = BuildKey(prefix);
        var db = redis.GetDatabase();
        var endpoints = redis.GetEndPoints();

        foreach (var server in endpoints.Select(e => redis.GetServer(e)))
        {
            var batch = new List<RedisKey>(DeleteChunkSize);

            await foreach (var key in server.KeysAsync(pattern: $"{fullPrefix}*").WithCancellation(ct))
            {
                batch.Add(key);
                if (batch.Count >= DeleteChunkSize)
                {
                    await db.KeyDeleteAsync(batch.ToArray()).ConfigureAwait(false);
                    batch.Clear();
                }
            }

            if (batch.Count > 0)
                await db.KeyDeleteAsync(batch.ToArray()).ConfigureAwait(false);
        }
    }
}
