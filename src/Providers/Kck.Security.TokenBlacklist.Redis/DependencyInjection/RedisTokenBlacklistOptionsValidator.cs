using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Kck.Security.TokenBlacklist.Redis.DependencyInjection;

/// <summary>
/// Validates <see cref="RedisTokenBlacklistOptions"/>: a connection string is required unless an
/// <see cref="IConnectionMultiplexer"/> is registered, which the blacklist then shares.
/// </summary>
public sealed class RedisTokenBlacklistOptionsValidator : IValidateOptions<RedisTokenBlacklistOptions>
{
    private readonly IServiceProviderIsService? _isService;

    /// <summary>Creates a validator that requires a connection string.</summary>
    public RedisTokenBlacklistOptionsValidator()
        : this(null)
    {
    }

    /// <summary>
    /// Creates a validator that accepts an empty connection string when <paramref name="isService"/> reports a
    /// registered <see cref="IConnectionMultiplexer"/>.
    /// </summary>
    /// <param name="isService">The container's service lookup; <see langword="null"/> requires a connection string.</param>
    public RedisTokenBlacklistOptionsValidator(IServiceProviderIsService? isService)
    {
        _isService = isService;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, RedisTokenBlacklistOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            return ValidateOptionsResult.Success;

        if (_isService?.IsService(typeof(IConnectionMultiplexer)) == true)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            """
            [Kck.Security.TokenBlacklist.Redis] RedisTokenBlacklistOptions is invalid:
              • ConnectionString: null or empty and no IConnectionMultiplexer is registered
                → Fix: opt.ConnectionString = "localhost:6379" or register Kck.Caching.Redis (AddKckCachingRedis) to share its connection
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md
            """);
    }
}
