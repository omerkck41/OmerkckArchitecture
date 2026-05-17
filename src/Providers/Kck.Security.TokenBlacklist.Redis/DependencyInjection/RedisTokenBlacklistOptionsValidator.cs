using Microsoft.Extensions.Options;

namespace Kck.Security.TokenBlacklist.Redis.DependencyInjection;

public sealed class RedisTokenBlacklistOptionsValidator : IValidateOptions<RedisTokenBlacklistOptions>
{
    public ValidateOptionsResult Validate(string? name, RedisTokenBlacklistOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            """
            [Kck.Security.TokenBlacklist.Redis] RedisTokenBlacklistOptions geçersiz:
              • ConnectionString: boş veya null
                → Fix: opt.ConnectionString = "localhost:6379"
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md
            """);
    }
}
