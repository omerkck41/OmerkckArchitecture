using StackExchange.Redis;

namespace Core.Security.JWT;

public class RedisTokenBlacklistManager : ITokenBlacklistManager
{
    private readonly IDatabase _redisDatabase;
    private const string BlacklistKeyPrefix = "jwt_blacklist_";

    public RedisTokenBlacklistManager(IConnectionMultiplexer redis)
    {
        _redisDatabase = redis.GetDatabase();
    }

    public void RevokeToken(string token, TimeSpan expiration)
    {
        _redisDatabase.StringSet(BlacklistKeyPrefix + token, "revoked", expiration);
    }

    public bool IsTokenRevoked(string token)
    {
        return _redisDatabase.KeyExists(BlacklistKeyPrefix + token);
    }
}