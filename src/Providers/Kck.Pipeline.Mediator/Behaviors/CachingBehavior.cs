using System.Text.Json;
using Kck.Core.Abstractions.Pipeline;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Caches responses for requests implementing <see cref="ICachableRequest"/>.
/// Uses <see cref="IDistributedCache"/> for provider flexibility.
/// </summary>
public sealed class CachingBehavior<TMessage, TResponse>(
    IDistributedCache cache,
    ILogger<CachingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IRequest<TResponse>, ICachableRequest
{
    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (message.BypassCache)
            return await next(message, cancellationToken).ConfigureAwait(false);

        var cacheKey = message.CacheKey;

        var cached = await cache.GetStringAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            Log.CacheHit(logger, cacheKey);
            return JsonSerializer.Deserialize<TResponse>(cached)!;
        }

        var response = await next(message, cancellationToken).ConfigureAwait(false);

        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = message.SlidingExpiration ?? TimeSpan.FromMinutes(5)
        };

        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), options, cancellationToken).ConfigureAwait(false);
        Log.CacheSet(logger, cacheKey);
        return response;
    }
}
