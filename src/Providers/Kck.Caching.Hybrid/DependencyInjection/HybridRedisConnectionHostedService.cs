using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Kck.Caching.Hybrid.DependencyInjection;

/// <summary>
/// Establishes the Redis <see cref="IConnectionMultiplexer"/> used by the hybrid cache asynchronously at host
/// startup, so no caller performs blocking I/O inside DI resolution or on the first request.
/// </summary>
internal sealed partial class HybridRedisConnectionHostedService(
    HybridRedisConnectionHolder holder,
    HybridRedisConnectionFactory factory,
    ILogger<HybridRedisConnectionHostedService> logger) : IHostedService, IAsyncDisposable
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var multiplexer = await factory.ConnectAsync(cancellationToken).ConfigureAwait(false);
        holder.SetMultiplexer(multiplexer);
        LogRedisConnected(logger);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Hybrid cache Redis connection established.")]
    private static partial void LogRedisConnected(ILogger logger);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public ValueTask DisposeAsync() => holder.DisposeAsync();
}

/// <summary>
/// Produces an <see cref="IConnectionMultiplexer"/> from a configuration string, <see cref="ConfigurationOptions"/>
/// or a user-supplied async factory.
/// </summary>
internal sealed class HybridRedisConnectionFactory(Func<CancellationToken, Task<IConnectionMultiplexer>> connector)
{
    public Task<IConnectionMultiplexer> ConnectAsync(CancellationToken cancellationToken)
        => connector(cancellationToken);
}
