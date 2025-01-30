using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;

namespace Core.Security.JWT;

/// <summary>
/// Redis tabanlı bir kara liste ile iptal edilen token'ları yönetir.
/// </summary>
public class RedisTokenBlacklist : ITokenBlacklist
{
    private readonly IDatabase _redisDatabase;
    private readonly TokenOptions _tokenOptions;

    /// <summary>
    /// <see cref="RedisTokenBlacklist"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="redisConnection">Redis bağlantı çoklayıcısı.</param>
    /// <param name="tokenOptions">JWT yapılandırma seçenekleri.</param>
    public RedisTokenBlacklist(IConnectionMultiplexer redisConnection, IOptions<TokenOptions> tokenOptions)
    {
        _redisDatabase = redisConnection.GetDatabase();
        _tokenOptions = tokenOptions.Value;
    }


    private async Task<TimeSpan?> GetTokenExpirationFromJwtAsync(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.ValidTo > DateTime.UtcNow ? jwtToken.ValidTo - DateTime.UtcNow : null;
    }
    /// <summary>
    /// Bir token'ı Redis kara listesine ekleyerek iptal eder.
    /// </summary>
    /// <param name="token">İptal edilecek token.</param>
    /// <param name="reason">İptal sebebi (opsiyonel).</param>
    public async Task RevokeTokenAsync(string token, string reason = "Revoked by system")
    {
        var expiration = await GetTokenExpirationFromJwtAsync(token);
        if (expiration == null) return;

        // Redis'e token'ı "revoked" olarak ekleyelim
        await _redisDatabase.StringSetAsync(token, reason, expiration);
    }

    /// <summary>
    /// Bir token'ın iptal edilip edilmediğini Redis kara listesinde kontrol eder.
    /// </summary>
    /// <param name="token">Kontrol edilecek token.</param>
    /// <returns>Token iptal edilmişse true; aksi takdirde false.</returns>
    public async Task<bool> IsTokenRevokedAsync(string token)
    {
        return await _redisDatabase.KeyExistsAsync(token);
    }
}