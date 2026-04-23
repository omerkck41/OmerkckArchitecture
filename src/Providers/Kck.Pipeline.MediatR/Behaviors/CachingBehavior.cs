using System.Text.Json;
using Kck.Core.Abstractions.Pipeline;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.MediatR.Behaviors;

/// <summary>
/// Caches responses for requests implementing <see cref="ICachableRequest"/>.
/// Uses <see cref="IDistributedCache"/> for provider flexibility (InMemory, Redis, etc.).
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICachableRequest
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request.BypassCache)
            return await next(cancellationToken);

        var cacheKey = request.CacheKey;

        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            Log.CacheHit(logger, cacheKey);
            return JsonSerializer.Deserialize<TResponse>(cached)!;
        }

        var response = await next(cancellationToken);

        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = request.SlidingExpiration ?? TimeSpan.FromMinutes(5)
        };

        var serialized = JsonSerializer.Serialize(response);
        await cache.SetStringAsync(cacheKey, serialized, options, cancellationToken);

        Log.CacheSet(logger, cacheKey);

        return response;
    }
}
