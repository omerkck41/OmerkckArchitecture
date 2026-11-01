using AwesomeAssertions;
using Kck.Security.TokenBlacklist.Redis.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace Kck.Security.TokenBlacklist.Redis.Tests;

public sealed class RedisTokenBlacklistOptionsValidatorTests
{
    private readonly RedisTokenBlacklistOptionsValidator _sut = new();

    [Fact]
    public void Validate_ValidConnectionString_ReturnsSuccess()
    {
        var opts = new RedisTokenBlacklistOptions { ConnectionString = "localhost:6379" };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyConnectionString_ReturnsFail(string cs)
    {
        var opts = new RedisTokenBlacklistOptions { ConnectionString = cs };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ConnectionString");
    }

    [Fact]
    public void Validate_EmptyConnectionString_WithRegisteredMultiplexer_ReturnsSuccess()
    {
        var isService = Substitute.For<IServiceProviderIsService>();
        isService.IsService(typeof(IConnectionMultiplexer)).Returns(true);
        var sut = new RedisTokenBlacklistOptionsValidator(isService);

        var result = sut.Validate(null, new RedisTokenBlacklistOptions { ConnectionString = "" });

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyConnectionString_WithoutMultiplexer_FailsAndMentionsSharing()
    {
        var isService = Substitute.For<IServiceProviderIsService>();
        var sut = new RedisTokenBlacklistOptionsValidator(isService);

        var result = sut.Validate(null, new RedisTokenBlacklistOptions { ConnectionString = "" });

        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("no IConnectionMultiplexer is registered")
            .And.Contain("AddKckCachingRedis");
    }
}
