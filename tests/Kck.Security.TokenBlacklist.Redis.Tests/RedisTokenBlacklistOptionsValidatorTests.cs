using FluentAssertions;
using Kck.Security.TokenBlacklist.Redis.DependencyInjection;
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
}
