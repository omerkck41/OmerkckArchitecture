using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Kck.Security.Abstractions.Token;

namespace Kck.Security.TokenBlacklist.Redis;

/// <summary>
/// Redis-based token blacklist using SETEX + EXISTS for O(1) revocation checks.
/// Keys auto-expire when the token's remaining lifetime elapses.
/// </summary>
public sealed class RedisTokenBlacklistService : ITokenBlacklistService, IDisposable
{
    private readonly RedisTokenBlacklistOptions _options;
    private readonly Lazy<ConnectionMultiplexer> _connection;

    public RedisTokenBlacklistService(IOptionsMonitor<RedisTokenBlacklistOptions> options)
    {
        _options = options.CurrentValue;

        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            throw new InvalidOperationException("Redis connection string is required for token blacklist.");

        _connection = new Lazy<ConnectionMultiplexer>(
            () => ConnectionMultiplexer.Connect(_options.ConnectionString));
    }

    /// <summary>
    /// Revokes a token by storing its JTI in Redis.
    /// Note: StackExchange.Redis does not support CancellationToken natively.
    /// The ct parameter is checked before the Redis call for early cancellation.
    /// </summary>
    public async Task RevokeAsync(string tokenId, TimeSpan expiration, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenId);
        ct.ThrowIfCancellationRequested();

        if (expiration <= TimeSpan.Zero)
            return;

        var db = GetDatabase();
        var key = BuildKey(tokenId);

        await db.StringSetAsync(key, "revoked", expiration).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if a token has been revoked.
    /// Note: StackExchange.Redis does not support CancellationToken natively.
    /// The ct parameter is checked before the Redis call for early cancellation.
    /// </summary>
    public async Task<bool> IsRevokedAsync(string tokenId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenId);
        ct.ThrowIfCancellationRequested();

        var db = GetDatabase();
        var key = BuildKey(tokenId);

        return await db.KeyExistsAsync(key).ConfigureAwait(false);
    }

    private IDatabase GetDatabase() => _connection.Value.GetDatabase(_options.Database);

    private string BuildKey(string tokenId) => $"{_options.KeyPrefix}{tokenId}";

    public void Dispose()
    {
        if (_connection.IsValueCreated)
            _connection.Value.Dispose();
    }
}
