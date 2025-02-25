using StackExchange.Redis;

namespace Core.Security.JWT;

public class RedisTokenBlacklistManager<TUserId> : ITokenBlacklistManager<TUserId>
{
    private readonly IDatabase _redisDatabase;

    public RedisTokenBlacklistManager(IConnectionMultiplexer redis)
    {
        _redisDatabase = redis.GetDatabase();
    }

    public void RevokeToken(string token, TUserId userId, TimeSpan expiration)
    {
        var key = TokenKeyHelper.BuildKey(userId, token);
        _redisDatabase.StringSet(key, "revoked", expiration);
    }

    public bool IsTokenRevoked(string token)
    {
        // Token'ın benzersiz olduğunu varsayarak, ilgili key'yi aramak için tüm userId'leri taramak yerine,
        // revoke işlemi sırasında token'ın hangi kullanıcıya ait olduğunu kaydedersen daha sağlıklı bir kontrol yapabilirsin.
        // Basit örnek için, pattern araması kullanılıyor (üretim ortamında performans açısından dikkatli kullanılmalı):
        var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
        var pattern = $"user_token_*_{token}";
        return server.Keys(pattern: pattern).Any();
    }

    public bool IsUserRevoked(TUserId userId)
    {
        var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
        var pattern = $"user_token_{userId}_*";
        return server.Keys(pattern: pattern).Any();
    }

    public void RemoveFromBlacklist(string token)
    {
        var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
        var pattern = $"user_token_*_{token}";
        foreach (var key in server.Keys(pattern: pattern))
        {
            _redisDatabase.KeyDelete(key);
        }
    }

    public void RemoveUserFromBlacklist(TUserId userId)
    {
        var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
        var pattern = $"user_token_{userId}_*";
        foreach (var key in server.Keys(pattern: pattern))
        {
            _redisDatabase.KeyDelete(key);
        }
    }
}