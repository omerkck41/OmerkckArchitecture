namespace Kck.Core.Abstractions.Pipeline;

/// <summary>Marks a pipeline request whose response should be cached by <c>CachingBehavior</c>.</summary>
public interface ICachableRequest
{
    /// <summary>Unique key used to store and retrieve the response in the cache store.</summary>
    string CacheKey { get; }

    /// <summary>When <see langword="true"/>, skips the cache read and forces a fresh response, then refreshes the cache entry.</summary>
    bool BypassCache { get; }

    /// <summary>Optional tag that groups related cache entries for bulk invalidation; <see langword="null"/> disables group tracking.</summary>
    string? CacheGroupKey { get; }

    /// <summary>Optional sliding TTL that resets on each cache hit; overrides <c>CacheOptions.DefaultExpiration</c> when set.</summary>
    TimeSpan? SlidingExpiration { get; }
}
