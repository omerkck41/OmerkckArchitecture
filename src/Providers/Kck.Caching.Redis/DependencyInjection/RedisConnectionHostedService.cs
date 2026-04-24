using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Kck.Caching.Redis.DependencyInjection;

/// <summary>
/// Establishes the shared Redis <see cref="IConnectionMultiplexer"/> asynchronously at host startup,
/// so no caller performs blocking I/O on first request.
/// </summary>
internal sealed partial class RedisConnectionHostedService(
    RedisConnectionHolder holder,
    RedisConnectionFactory factory,
    ILogger<RedisConnectionHostedService> logger) : IHostedService, IAsyncDisposable
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var multiplexer = await factory.ConnectAsync(cancellationToken).ConfigureAwait(false);
        holder.SetMultiplexer(multiplexer);
        LogRedisConnected(logger);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Redis connection established.")]
    private static partial void LogRedisConnected(ILogger logger);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public ValueTask DisposeAsync() => holder.DisposeAsync();
}

/// <summary>
/// Produces an <see cref="IConnectionMultiplexer"/> from either a configuration string
/// or a user-supplied async factory.
/// </summary>
internal sealed class RedisConnectionFactory(Func<CancellationToken, Task<IConnectionMultiplexer>> connector)
{
    public Task<IConnectionMultiplexer> ConnectAsync(CancellationToken cancellationToken)
        => connector(cancellationToken);
}
