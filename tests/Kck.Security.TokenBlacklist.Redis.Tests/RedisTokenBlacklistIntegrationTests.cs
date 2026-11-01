using AwesomeAssertions;
using Kck.Security.TokenBlacklist.Redis;
using Kck.Testing;
using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

namespace Kck.Security.TokenBlacklist.Redis.Tests;

/// <summary>
/// LS-FAZ-4.5: Real Redis container integration tests for token blacklisting.
/// Validates SETEX write and KeyExists read through a real Redis instance.
/// </summary>
/// <remarks>
/// Requires Docker. CI: ubuntu-only (Category=Integration filter).
/// </remarks>
[Trait("Category", "Integration")]
#pragma warning disable CA1001
public sealed class RedisTokenBlacklistIntegrationTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .Build();

    private RedisTokenBlacklistService _sut = default!;

    public async Task InitializeAsync()
    {
        await _redis.StartAsync();

        var options = new StaticOptionsMonitor<RedisTokenBlacklistOptions>(
            new RedisTokenBlacklistOptions
            {
                ConnectionString = _redis.GetConnectionString(),
                KeyPrefix = "bl:",
                Database = 0
            });

        _sut = new RedisTokenBlacklistService(options);
    }

    public async Task DisposeAsync()
    {
        _sut.Dispose();
        await _redis.DisposeAsync();
    }

    [Fact]
    public async Task RevokeAsync_IsRevokedAsync_RoundTrip()
    {
        var jti = Guid.NewGuid().ToString();

        await _sut.RevokeAsync(jti, TimeSpan.FromMinutes(15));
        var isRevoked = await _sut.IsRevokedAsync(jti);

        isRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task IsRevokedAsync_ForUnknownToken_ReturnsFalse()
    {
        var result = await _sut.IsRevokedAsync(Guid.NewGuid().ToString());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeAsync_WithZeroExpiration_DoesNotStoreToken()
    {
        var jti = Guid.NewGuid().ToString();

        await _sut.RevokeAsync(jti, TimeSpan.Zero);
        var isRevoked = await _sut.IsRevokedAsync(jti);

        isRevoked.Should().BeFalse("zero expiration should be treated as a no-op");
    }

    [Fact]
    public async Task RevokeAsync_WithSharedMultiplexer_RoundTrip()
    {
        await using var connection = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
        var options = new StaticOptionsMonitor<RedisTokenBlacklistOptions>(
            new RedisTokenBlacklistOptions { KeyPrefix = "shared:", Database = 0 });
        var jti = Guid.NewGuid().ToString();

        using (var sut = new RedisTokenBlacklistService(options, connection))
        {
            await sut.RevokeAsync(jti, TimeSpan.FromMinutes(15));
            (await sut.IsRevokedAsync(jti)).Should().BeTrue();
        }

        // The service does not own the shared connection, so disposing it leaves the connection usable.
        (await connection.GetDatabase(0).KeyExistsAsync($"shared:{jti}")).Should().BeTrue();
    }
}
