using AwesomeAssertions;
using Kck.Security.Abstractions.Token;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace Kck.Security.TokenBlacklist.Redis.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddKckTokenBlacklistRedis_WithEmptyConnectionString_UsesRegisteredMultiplexer()
    {
        var connection = Substitute.For<IConnectionMultiplexer>();
        var db = Substitute.For<IDatabase>();
        connection.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(db);
        db.KeyExistsAsync(new RedisKey("bl:jti"), Arg.Any<CommandFlags>()).Returns(true);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(connection);
        services.AddKckTokenBlacklistRedis(opt => opt.KeyPrefix = "bl:");
        await using var provider = services.BuildServiceProvider();

        var result = await provider.GetRequiredService<ITokenBlacklistService>().IsRevokedAsync("jti");

        result.Should().BeTrue();
        await db.Received(1).KeyExistsAsync(new RedisKey("bl:jti"), Arg.Any<CommandFlags>());
    }

    [Fact]
    public async Task AddKckTokenBlacklistRedis_WithoutMultiplexerAndEmptyConnectionString_FailsValidation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddKckTokenBlacklistRedis(opt => opt.KeyPrefix = "bl:");
        await using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IOptionsMonitor<RedisTokenBlacklistOptions>>().CurrentValue;

        act.Should().Throw<OptionsValidationException>();
    }
}
