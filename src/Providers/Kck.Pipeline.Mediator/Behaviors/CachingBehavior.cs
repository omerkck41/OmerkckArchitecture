using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Kck.Pipeline.Abstractions;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Caches responses for requests implementing <see cref="ICachableRequest"/>.
/// Uses <see cref="IDistributedCache"/> for provider flexibility.
/// </summary>
public sealed class CachingBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IRequest<TResponse>, ICachableRequest
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TMessage, TResponse>> _logger;
    private readonly JsonSerializerOptions? _serializerOptions;

    /// <summary>Creates the behavior with the default JSON settings.</summary>
    public CachingBehavior(IDistributedCache cache, ILogger<CachingBehavior<TMessage, TResponse>> logger)
        : this(cache, logger, Options.Create(new CachingBehaviorOptions()))
    {
    }

    /// <summary>Creates the behavior with the configured <see cref="CachingBehaviorOptions"/>.</summary>
    public CachingBehavior(
        IDistributedCache cache,
        ILogger<CachingBehavior<TMessage, TResponse>> logger,
        IOptions<CachingBehaviorOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _cache = cache;
        _logger = logger;
        _serializerOptions = options.Value.SerializerOptions;
    }

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (message.BypassCache)
            return await next(message, cancellationToken).ConfigureAwait(false);

        var cacheKey = message.CacheKey;
        // Metadata comes from the options' resolver: reflection by default, a source-generated context under AOT.
        var serializerOptions = _serializerOptions ?? CachingBehaviorOptions.DefaultSerializerOptions();
        var typeInfo = (JsonTypeInfo<TResponse>)serializerOptions.GetTypeInfo(typeof(TResponse));

        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            Log.CacheHit(_logger, cacheKey);
            return JsonSerializer.Deserialize(cached, typeInfo)!;
        }

        var response = await next(message, cancellationToken).ConfigureAwait(false);

        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = message.SlidingExpiration ?? TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response, typeInfo), options, cancellationToken).ConfigureAwait(false);
        Log.CacheSet(_logger, cacheKey);
        return response;
    }
}
