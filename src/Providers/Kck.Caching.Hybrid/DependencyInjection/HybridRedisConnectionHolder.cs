using StackExchange.Redis;

namespace Kck.Caching.Hybrid.DependencyInjection;

/// <summary>
/// Holds the <see cref="IConnectionMultiplexer"/> established asynchronously during application startup by
/// <see cref="HybridRedisConnectionHostedService"/> (same pattern as Kck.Caching.Redis).
/// </summary>
internal sealed class HybridRedisConnectionHolder
{
    private IConnectionMultiplexer? _multiplexer;

    public IConnectionMultiplexer Multiplexer =>
        _multiplexer ?? throw new InvalidOperationException(
            "Kck.Caching.Hybrid: Redis connection has not been initialized. " +
            "Ensure the host's StartAsync has completed before resolving ICacheService or IConnectionMultiplexer.");

    public void SetMultiplexer(IConnectionMultiplexer multiplexer)
    {
        if (_multiplexer is not null)
            throw new InvalidOperationException("Kck.Caching.Hybrid: Redis connection is already initialized.");
        _multiplexer = multiplexer;
    }

    public async ValueTask DisposeAsync()
    {
        if (_multiplexer is not null)
        {
            await _multiplexer.DisposeAsync().ConfigureAwait(false);
            _multiplexer = null;
        }
    }
}
