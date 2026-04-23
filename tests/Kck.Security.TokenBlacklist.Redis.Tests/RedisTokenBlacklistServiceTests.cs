using FluentAssertions;
using Kck.Testing;
using Xunit;

namespace Kck.Security.TokenBlacklist.Redis.Tests;

public class RedisTokenBlacklistServiceTests
{
    [Fact]
    public void Constructor_WithEmptyConnectionString_ShouldThrow()
    {
        var options = new StaticOptionsMonitor<RedisTokenBlacklistOptions>(new RedisTokenBlacklistOptions
        {
            ConnectionString = ""
        });

        var act = () => new RedisTokenBlacklistService(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*connection string*");
    }

    [Fact]
    public void Constructor_WithValidConnectionString_ShouldNotThrow()
    {
        var options = new StaticOptionsMonitor<RedisTokenBlacklistOptions>(new RedisTokenBlacklistOptions
        {
            ConnectionString = "localhost:6379",
            KeyPrefix = "test:blacklist:"
        });

        var act = () => new RedisTokenBlacklistService(options);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task RevokeAsync_WithNullTokenId_ShouldThrow()
    {
        var options = new StaticOptionsMonitor<RedisTokenBlacklistOptions>(new RedisTokenBlacklistOptions
        {
            ConnectionString = "localhost:6379"
        });
        var sut = new RedisTokenBlacklistService(options);

        var act = () => sut.RevokeAsync(null!, TimeSpan.FromMinutes(15));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RevokeAsync_WithCancelledToken_ShouldThrow()
    {
        var options = new StaticOptionsMonitor<RedisTokenBlacklistOptions>(new RedisTokenBlacklistOptions
        {
            ConnectionString = "localhost:6379"
        });
        var sut = new RedisTokenBlacklistService(options);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => sut.RevokeAsync("test-jti", TimeSpan.FromMinutes(15), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task IsRevokedAsync_WithCancelledToken_ShouldThrow()
    {
        var options = new StaticOptionsMonitor<RedisTokenBlacklistOptions>(new RedisTokenBlacklistOptions
        {
            ConnectionString = "localhost:6379"
        });
        var sut = new RedisTokenBlacklistService(options);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => sut.IsRevokedAsync("test-jti", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
