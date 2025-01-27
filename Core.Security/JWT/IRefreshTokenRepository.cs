namespace Core.Security.JWT;

public interface IRefreshTokenRepository<TId, TUserId>
{
    /// <summary>
    /// Belirli bir kullanıcı için geçerli olan refresh token'ı alır.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si.</param>
    Task<RefreshToken<TId, TUserId>> GetCurrentRefreshTokenAsync(TUserId userId);

    /// <summary>
    /// Yeni bir refresh token ekler.
    /// </summary>
    /// <param name="refreshToken">Eklenecek refresh token.</param>
    Task AddRefreshTokenAsync(RefreshToken<TId, TUserId> refreshToken);

    /// <summary>
    /// Mevcut bir refresh token'ı iptal eder.
    /// </summary>
    /// <param name="tokenId">Token ID'si.</param>
    /// <param name="revokedByIp">İptal eden IP adresi.</param>
    /// <param name="reasonRevoked">İptal sebebi.</param>
    /// <param name="replacedByToken">Yerine geçen token (isteğe bağlı).</param>
    Task RevokeRefreshTokenAsync(TId tokenId, string revokedByIp, string reasonRevoked, string replacedByToken = null);

    /// <summary>
    /// Belirli bir kullanıcıya ait tüm refresh token'ları alır.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si.</param>
    Task<IEnumerable<RefreshToken<TId, TUserId>>> GetAllRefreshTokensAsync(TUserId userId);

    /// <summary>
    /// Süresi dolmuş refresh token'ları kaldırır.
    /// </summary>
    Task RemoveExpiredRefreshTokensAsync();
}