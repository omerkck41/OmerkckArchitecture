namespace Kck.Pipeline.Abstractions;

/// <summary>
/// Marks a pipeline request whose response should be cached.
/// Implement this interface on a request to activate <c>CachingBehavior</c>.
/// </summary>
public interface ICachableRequest
{
    /// <summary>The cache key under which the response is stored.</summary>
    string CacheKey { get; }

    /// <summary>When <see langword="true"/>, bypasses the cache and refreshes the stored value.</summary>
    bool BypassCache { get; }

    /// <summary>Optional group key for bulk cache invalidation.</summary>
    string? CacheGroupKey { get; }

    /// <summary>Optional sliding expiration for the cached entry. Uses provider default when <see langword="null"/>.</summary>
    TimeSpan? SlidingExpiration { get; }
}
