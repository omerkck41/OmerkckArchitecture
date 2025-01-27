using Newtonsoft.Json;
using StackExchange.Redis;

namespace Core.Security.JWT;

/// <summary>
/// Redis tabanlı bir refresh token deposu temsil eder.
/// </summary>
/// <typeparam name="TId">Token'ın benzersiz kimlik türü.</typeparam>
/// <typeparam name="TUserId">Kullanıcının benzersiz kimlik türü.</typeparam>
public class RedisRefreshTokenRepository<TId, TUserId> : IRefreshTokenRepository<TId, TUserId>
{
    private readonly IDatabase _redisDatabase;
    private readonly IServer _redisServer;
    private readonly TokenOptions _tokenOptions;

    /// <summary>
    /// <see cref="RedisRefreshTokenRepository{TId, TUserId}"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="redisConnection">Redis bağlantı çoklayıcısı.</param>
    /// <param name="tokenOptions">Token yapılandırma seçenekleri.</param>
    public RedisRefreshTokenRepository(IConnectionMultiplexer redisConnection, TokenOptions tokenOptions)
    {
        _redisDatabase = redisConnection.GetDatabase();
        _redisServer = redisConnection.GetServer(redisConnection.GetEndPoints().First());
        _tokenOptions = tokenOptions;
    }

    /// <summary>
    /// Yeni bir refresh token'ı Redis'e ekler.
    /// </summary>
    /// <param name="refreshToken">Eklenecek refresh token.</param>
    public async Task AddRefreshTokenAsync(RefreshToken<TId, TUserId> refreshToken)
    {
        var key = $"RefreshToken:{refreshToken.UserId}:{refreshToken.Id}";
        await _redisDatabase.StringSetAsync(key, JsonConvert.SerializeObject(refreshToken), TimeSpan.FromDays(_tokenOptions.RefreshTokenTTL));
    }

    /// <summary>
    /// Belirli bir kullanıcı için geçerli refresh token'ı Redis'ten getirir.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si.</param>
    /// <returns>Geçerli refresh token; bulunamazsa null.</returns>
    public async Task<RefreshToken<TId, TUserId>> GetCurrentRefreshTokenAsync(TUserId userId)
    {
        var keys = _redisServer.Keys(pattern: $"RefreshToken:{userId}:*").ToList();
        foreach (var key in keys)
        {
            var tokenJson = await _redisDatabase.StringGetAsync(key);
            var token = JsonConvert.DeserializeObject<RefreshToken<TId, TUserId>>(tokenJson);
            if (token.ExpirationDate > DateTime.UtcNow && token.RevokedDate == null)
            {
                return token;
            }
        }
        return null;
    }

    /// <summary>
    /// Bir refresh token'ı iptal eder.
    /// </summary>
    /// <param name="tokenId">Token ID'si.</param>
    /// <param name="revokedByIp">İptal eden IP adresi.</param>
    /// <param name="reasonRevoked">İptal sebebi.</param>
    /// <param name="replacedByToken">Yerine geçen token (isteğe bağlı).</param>
    public async Task RevokeRefreshTokenAsync(TId tokenId, string revokedByIp, string reasonRevoked, string replacedByToken = null)
    {
        var key = $"RefreshToken:{tokenId}";
        var tokenJson = await _redisDatabase.StringGetAsync(key);
        if (tokenJson.HasValue)
        {
            var token = JsonConvert.DeserializeObject<RefreshToken<TId, TUserId>>(tokenJson);
            token.Revoke(revokedByIp, reasonRevoked, replacedByToken);
            await _redisDatabase.StringSetAsync(key, JsonConvert.SerializeObject(token));
        }
    }

    /// <summary>
    /// Belirli bir kullanıcıya ait tüm refresh token'ları alır.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si.</param>
    /// <returns>Refresh token listesi.</returns>
    public async Task<IEnumerable<RefreshToken<TId, TUserId>>> GetAllRefreshTokensAsync(TUserId userId)
    {
        var keys = _redisServer.Keys(pattern: $"RefreshToken:{userId}:*").ToList();
        var tokens = new List<RefreshToken<TId, TUserId>>();
        foreach (var key in keys)
        {
            var tokenJson = await _redisDatabase.StringGetAsync(key);
            tokens.Add(JsonConvert.DeserializeObject<RefreshToken<TId, TUserId>>(tokenJson));
        }
        return tokens;
    }

    /// <summary>
    /// Süresi dolmuş tüm refresh token'ları kaldırır.
    /// </summary>
    public async Task RemoveExpiredRefreshTokensAsync()
    {
        var keys = _redisServer.Keys(pattern: "RefreshToken:*").ToList();
        foreach (var key in keys)
        {
            var tokenJson = await _redisDatabase.StringGetAsync(key);
            var token = JsonConvert.DeserializeObject<RefreshToken<TId, TUserId>>(tokenJson);
            if (token.ExpirationDate < DateTime.UtcNow)
            {
                await _redisDatabase.KeyDeleteAsync(key);
            }
        }
    }
}