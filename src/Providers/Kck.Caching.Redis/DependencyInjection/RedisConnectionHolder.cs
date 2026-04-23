using StackExchange.Redis;

namespace Kck.Caching.Redis.DependencyInjection;

/// <summary>
/// Holds a lazily-initialized <see cref="IConnectionMultiplexer"/> established asynchronously
/// during application startup by <see cref="RedisConnectionHostedService"/>.
/// </summary>
internal sealed class RedisConnectionHolder
{
    private IConnectionMultiplexer? _multiplexer;

    public IConnectionMultiplexer Multiplexer =>
        _multiplexer ?? throw new InvalidOperationException(
            "Redis connection has not been initialized. " +
            "Ensure the host's StartAsync has completed before resolving IConnectionMultiplexer.");

    public void SetMultiplexer(IConnectionMultiplexer multiplexer)
    {
        if (_multiplexer is not null)
            throw new InvalidOperationException("Redis connection is already initialized.");
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
