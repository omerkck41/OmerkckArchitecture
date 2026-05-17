namespace Kck.Caching.Abstractions;

/// <summary>Configuration for <see cref="ICacheService"/> implementations, bound from application settings.</summary>
public sealed class CacheOptions
{
    /// <summary>Default TTL applied when no per-call expiration is specified; defaults to 5 minutes.</summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Optional prefix prepended to every cache key for namespace isolation across environments or tenants.</summary>
    public string? KeyPrefix { get; set; }
}
