using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Kck.Security.Abstractions.Token;

namespace Kck.Security.TokenBlacklist.Redis;

/// <summary>
/// Redis-based token blacklist using SETEX + EXISTS for O(1) revocation checks.
/// Keys auto-expire when the token's remaining lifetime elapses.
/// </summary>
/// <remarks>
/// Either uses a shared <see cref="IConnectionMultiplexer"/> (not owned, never disposed here) or opens its own
/// connection asynchronously on first use. Concurrent first callers share one connect attempt; when it fails they all
/// fail with its exception, and the failure is not cached: the next call starts a new attempt. After
/// <see cref="Dispose"/> every call throws <see cref="ObjectDisposedException"/>.
/// </remarks>
public sealed class RedisTokenBlacklistService : ITokenBlacklistService, IDisposable
{
    private readonly RedisTokenBlacklistOptions _options;
    private readonly Func<string, Task<IConnectionMultiplexer>>? _connect;
    // Only WaitAsync is used, so the semaphore never allocates a wait handle and is not disposed (as in
    // AzureBlobStorageService): disposing it would turn a call racing with Dispose into a different exception.
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly bool _ownsConnection;
    private IConnectionMultiplexer? _connection;
    // The in-flight connect attempt shared by concurrent callers; written under _connectLock.
    private Task<IConnectionMultiplexer>? _connecting;
    private volatile bool _disposed;

    /// <summary>
    /// Creates the service with its own Redis connection, opened on first use from
    /// <see cref="RedisTokenBlacklistOptions.ConnectionString"/>.
    /// </summary>
    /// <param name="options">The blacklist options; <c>ConnectionString</c> is required.</param>
    /// <exception cref="InvalidOperationException">The connection string is empty.</exception>
    public RedisTokenBlacklistService(IOptionsMonitor<RedisTokenBlacklistOptions> options)
        : this(options, static async cs => await ConnectionMultiplexer.ConnectAsync(cs).ConfigureAwait(false))
    {
    }

    /// <summary>
    /// Creates the service on a shared Redis connection (for example the one registered by Kck.Caching.Redis).
    /// The connection is not owned: <see cref="Dispose"/> leaves it open.
    /// </summary>
    /// <param name="options">The blacklist options; <c>ConnectionString</c> is ignored and may be empty.</param>
    /// <param name="connection">The shared connection.</param>
    public RedisTokenBlacklistService(
        IOptionsMonitor<RedisTokenBlacklistOptions> options, IConnectionMultiplexer connection)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(connection);

        _options = options.CurrentValue;
        _connection = connection;
    }

    // Own-connection path with an injectable connect function (test seam for the public ctor).
    internal RedisTokenBlacklistService(
        IOptionsMonitor<RedisTokenBlacklistOptions> options, Func<string, Task<IConnectionMultiplexer>> connect)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(connect);

        _options = options.CurrentValue;

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            throw new InvalidOperationException("Redis connection string is required for token blacklist.");

        _connect = connect;
        _ownsConnection = true;
    }

    /// <summary>
    /// Revokes a token by storing its JTI in Redis.
    /// Note: StackExchange.Redis does not support CancellationToken natively.
    /// The ct parameter is checked before the Redis call for early cancellation.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The service was disposed.</exception>
    public async Task RevokeAsync(string tokenId, TimeSpan expiration, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenId);
        ct.ThrowIfCancellationRequested();

        if (expiration <= TimeSpan.Zero)
            return;

        var db = await GetDatabaseAsync(ct).ConfigureAwait(false);
        var key = BuildKey(tokenId);

        await db.StringSetAsync(key, "revoked", expiration).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if a token has been revoked.
    /// Note: StackExchange.Redis does not support CancellationToken natively.
    /// The ct parameter is checked before the Redis call for early cancellation.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The service was disposed.</exception>
    public async Task<bool> IsRevokedAsync(string tokenId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenId);
        ct.ThrowIfCancellationRequested();

        var db = await GetDatabaseAsync(ct).ConfigureAwait(false);
        var key = BuildKey(tokenId);

        return await db.KeyExistsAsync(key).ConfigureAwait(false);
    }

    private async ValueTask<IDatabase> GetDatabaseAsync(CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var connection = Volatile.Read(ref _connection);
        if (connection is not null)
            return connection.GetDatabase(_options.Database);

        Task<IConnectionMultiplexer> attempt;
        await _connectLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            connection = Volatile.Read(ref _connection);
            if (connection is not null)
                return connection.GetDatabase(_options.Database);

            // Join the attempt in flight, or start one. Every caller awaits the same task outside the lock.
            attempt = _connecting ??= ConnectAsync();
        }
        finally
        {
            _connectLock.Release();
        }

        connection = await attempt.WaitAsync(ct).ConfigureAwait(false);
        ObjectDisposedException.ThrowIf(_disposed, this);
        return connection.GetDatabase(_options.Database);
    }

    // One connect attempt. On success the connection is published; on failure the attempt is cleared so the next call
    // starts a new one, while the callers that joined this attempt all observe its exception.
    private async Task<IConnectionMultiplexer> ConnectAsync()
    {
        IConnectionMultiplexer connection;
        try
        {
            connection = await _connect!(_options.ConnectionString).ConfigureAwait(false);
        }
        catch
        {
            await ClearAttemptAsync().ConfigureAwait(false);
            throw;
        }

        Volatile.Write(ref _connection, connection);
        await ClearAttemptAsync().ConfigureAwait(false);

        // Dispose ran while the connect was in flight and found no connection to close: close this one now.
        if (_disposed)
        {
            Interlocked.Exchange(ref _connection, null)?.Dispose();
            throw new ObjectDisposedException(GetType().FullName);
        }

        return connection;
    }

    private async Task ClearAttemptAsync()
    {
        await _connectLock.WaitAsync().ConfigureAwait(false);
        try
        {
            _connecting = null;
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private string BuildKey(string tokenId) => $"{_options.KeyPrefix}{tokenId}";

    /// <summary>
    /// Disposes the own connection, if one was opened (a connect still in flight disposes its connection when it
    /// completes); a shared connection is left open. Later calls throw <see cref="ObjectDisposedException"/>.
    /// </summary>
    public void Dispose()
    {
        _disposed = true;

        if (_ownsConnection)
            Interlocked.Exchange(ref _connection, null)?.Dispose();
    }
}
