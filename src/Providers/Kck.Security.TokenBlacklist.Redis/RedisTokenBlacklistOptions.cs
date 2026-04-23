namespace Kck.Security.TokenBlacklist.Redis;

public sealed class RedisTokenBlacklistOptions
{
    /// <summary>Redis connection string. Required.</summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>Key prefix for blacklisted tokens. Default: "kck:token:blacklist:"</summary>
    public string KeyPrefix { get; set; } = "kck:token:blacklist:";

    /// <summary>Redis database index. Default: -1 (default database).</summary>
    public int Database { get; set; } = -1;
}
