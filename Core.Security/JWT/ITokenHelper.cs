using Core.Security.Entities;
using System.Security.Claims;

namespace Core.Security.JWT;

public interface ITokenHelper<TUserId, TOperationClaimId, TRefreshTokenId>
{
    /// <summary>
    /// Kullanıcı için bir access token oluşturur.
    /// </summary>
    /// <param name="user">Token oluşturulacak kullanıcı.</param>
    /// <param name="operationClaims">Kullanıcının operasyon yetkileri.</param>
    /// <param name="customClaims">Ekstra claim'ler (isteğe bağlı).</param>
    AccessToken CreateToken(User<TUserId> user, IList<OperationClaim<TOperationClaimId>> operationClaims, IDictionary<string, string>? customClaims = null);

    /// <summary>
    /// Yeni bir refresh token oluşturur.
    /// </summary>
    /// <param name="user">Refresh token oluşturulacak kullanıcı.</param>
    /// <param name="ipAddress">IP adresi.</param>
    RefreshToken<TRefreshTokenId, TUserId> CreateRefreshToken(User<TUserId> user, string ipAddress);

    void RevokeToken(string token, TUserId userId, TimeSpan? expiration = null);

    /// <summary>
    /// Bir token'ın geçerliliğini kontrol eder.
    /// </summary>
    /// <param name="token">Kontrol edilecek token.</param>
    bool ValidateToken(string token);

    /// <summary>
    /// Bir token'dan claim'leri alır.
    /// </summary>
    /// <param name="token">Claim'leri alınacak token.</param>
    IEnumerable<Claim> GetClaimsFromToken(string token);

    /// <summary>
    /// Bir token'dan kullanıcı ID'sini alır.
    /// </summary>
    /// <param name="token">Kullanıcı ID'si alınacak token.</param>
    TUserId GetUserIdFromToken(string token);

    /// <summary>
    /// Bir token'ın son kullanma tarihini alır.
    /// </summary>
    /// <param name="token">Son kullanma tarihi alınacak token.</param>
    DateTime GetExpirationDateFromToken(string token);
}