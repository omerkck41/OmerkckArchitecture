namespace Kck.Core.Abstractions.Pipeline;

public interface ICachableRequest
{
    string CacheKey { get; }
    bool BypassCache { get; }
    string? CacheGroupKey { get; }
    TimeSpan? SlidingExpiration { get; }
}
