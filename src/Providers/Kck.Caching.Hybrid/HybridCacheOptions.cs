namespace Kck.Caching.Hybrid;

/// <summary>
/// Configuration for the HybridCache provider (L1 in-memory + L2 Redis).
/// </summary>
public sealed class HybridCacheOptions
{
    /// <summary>Optional key prefix applied to every cache entry.</summary>
    public string? KeyPrefix { get; set; }

    /// <summary>Default L1+L2 expiration. Default: 5 minutes.</summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Default L1 (in-process memory) entry lifetime.
    /// Must be ≤ <see cref="DefaultExpiration"/>. Default: 1 minute.
    /// </summary>
    public TimeSpan DefaultLocalExpiration { get; set; } = TimeSpan.FromMinutes(1);
}
