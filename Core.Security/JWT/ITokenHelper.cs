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
    Task<AccessToken> CreateTokenAsync(User<TUserId> user, IList<OperationClaim<TOperationClaimId>> operationClaims, IDictionary<string, string> customClaims = null);

    /// <summary>
    /// Yeni bir refresh token oluşturur.
    /// </summary>
    /// <param name="user">Refresh token oluşturulacak kullanıcı.</param>
    /// <param name="ipAddress">IP adresi.</param>
    Task<RefreshToken<TRefreshTokenId, TUserId>> CreateRefreshTokenAsync(User<TUserId> user, string ipAddress);

    /// <summary>
    /// Bir access token'ı yeniler.
    /// </summary>
    /// <param name="user">Token yenilenecek kullanıcı.</param>
    /// <param name="operationClaims">Kullanıcının operasyon yetkileri.</param>
    /// <param name="ipAddress">IP adresi.</param>
    Task<AccessToken> RefreshTokenAsync(User<TUserId> user, IList<OperationClaim<TOperationClaimId>> operationClaims, string ipAddress);

    /// <summary>
    /// Bir token'ın geçerliliğini kontrol eder.
    /// </summary>
    /// <param name="token">Kontrol edilecek token.</param>
    Task<bool> ValidateTokenAsync(string token);

    /// <summary>
    /// Bir token'ı iptal eder.
    /// </summary>
    /// <param name="token">İptal edilecek token.</param>
    Task RevokeTokenAsync(string token);

    /// <summary>
    /// Bir token'dan claim'leri alır.
    /// </summary>
    /// <param name="token">Claim'leri alınacak token.</param>
    Task<IEnumerable<Claim>> GetClaimsFromTokenAsync(string token);

    /// <summary>
    /// Bir token'dan kullanıcı ID'sini alır.
    /// </summary>
    /// <param name="token">Kullanıcı ID'si alınacak token.</param>
    Task<TUserId> GetUserIdFromTokenAsync(string token);

    /// <summary>
    /// Bir token'ın son kullanma tarihini alır.
    /// </summary>
    /// <param name="token">Son kullanma tarihi alınacak token.</param>
    Task<DateTime> GetExpirationDateFromTokenAsync(string token);
}