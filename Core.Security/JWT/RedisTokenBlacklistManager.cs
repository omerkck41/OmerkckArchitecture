using StackExchange.Redis;

namespace Core.Security.JWT;

public class RedisTokenBlacklistManager<TUserId> : ITokenBlacklistManager<TUserId>
{
    private readonly IDatabase _redisDatabase;
    private const string BlacklistKeyPrefix = "user_token_";

    public RedisTokenBlacklistManager(IConnectionMultiplexer redis)
    {
        _redisDatabase = redis.GetDatabase();
    }

    public void RevokeToken(string token, TUserId userId, TimeSpan expiration)
    {
        _redisDatabase.StringSet($"{BlacklistKeyPrefix}{userId}_{token}", "revoked", expiration);
    }

    public bool IsTokenRevoked(string token)
    {
        return _redisDatabase.KeyExists($"{BlacklistKeyPrefix}_*_" + token);
    }

    public bool IsUserRevoked(TUserId userId)
    {
        return _redisDatabase.KeyExists($"{BlacklistKeyPrefix}{userId}_*");
    }

    public void RemoveFromBlacklist(string token)
    {
        _redisDatabase.KeyDelete($"token_{token}");
    }

    public void RemoveUserFromBlacklist(TUserId userId)
    {
        _redisDatabase.KeyDelete($"user_{userId}");
    }
}