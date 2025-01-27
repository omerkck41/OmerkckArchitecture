using StackExchange.Redis;

namespace Core.Security.JWT;

/// <summary>
/// Redis tabanlı bir kara liste ile iptal edilen token'ları yönetir.
/// </summary>
public class RedisTokenBlacklist : ITokenBlacklist
{
    private readonly IDatabase _redisDatabase;

    /// <summary>
    /// <see cref="RedisTokenBlacklist"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="redisConnection">Redis bağlantı çoklayıcısı.</param>
    public RedisTokenBlacklist(IConnectionMultiplexer redisConnection)
    {
        _redisDatabase = redisConnection.GetDatabase();
    }

    /// <summary>
    /// Bir token'ı Redis kara listesine ekleyerek iptal eder.
    /// </summary>
    /// <param name="token">İptal edilecek token.</param>
    public void RevokeToken(string token)
    {
        _redisDatabase.StringSet(token, "revoked", TimeSpan.FromMinutes(60)); // Token'ı 60 dakika boyunca iptal et
    }

    /// <summary>
    /// Bir token'ın iptal edilip edilmediğini Redis kara listesinde kontrol eder.
    /// </summary>
    /// <param name="token">Kontrol edilecek token.</param>
    /// <returns>Token iptal edilmişse true; aksi takdirde false.</returns>
    public bool IsTokenRevoked(string token)
    {
        return _redisDatabase.KeyExists(token);
    }
}