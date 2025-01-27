namespace Core.Security.JWT;

/// <summary>
/// Bellek üzerinde çalışan bir refresh token deposu temsil eder.
/// </summary>
/// <typeparam name="TId">Token'ın benzersiz kimlik türü.</typeparam>
/// <typeparam name="TUserId">Kullanıcının benzersiz kimlik türü.</typeparam>
public class RefreshTokenRepository<TId, TUserId> : IRefreshTokenRepository<TId, TUserId>
{
    private readonly List<RefreshToken<TId, TUserId>> _refreshTokens = new();

    /// <summary>
    /// Belirli bir kullanıcı için geçerli refresh token'ı getirir.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si.</param>
    /// <returns>Geçerli refresh token; bulunamazsa null.</returns>
    public async Task<RefreshToken<TId, TUserId>> GetCurrentRefreshTokenAsync(TUserId userId)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.UserId.Equals(userId) && rt.ExpirationDate > DateTime.UtcNow && rt.RevokedDate == null);
        return await Task.FromResult(refreshToken);
    }

    /// <summary>
    /// Yeni bir refresh token'ı depoya ekler.
    /// </summary>
    /// <param name="refreshToken">Eklenecek refresh token.</param>
    public async Task AddRefreshTokenAsync(RefreshToken<TId, TUserId> refreshToken)
    {
        _refreshTokens.Add(refreshToken);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Belirli bir refresh token'ı iptal eder.
    /// </summary>
    /// <param name="tokenId">İptal edilecek token'ın ID'si.</param>
    /// <param name="revokedByIp">Token'ı iptal eden IP adresi.</param>
    /// <param name="reasonRevoked">Token'ın iptal edilme nedeni.</param>
    /// <param name="replacedByToken">Bu token'ın yerine geçen token (opsiyonel).</param>
    public async Task RevokeRefreshTokenAsync(TId tokenId, string revokedByIp, string reasonRevoked, string replacedByToken = null)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.Id.Equals(tokenId));
        if (refreshToken != null)
        {
            refreshToken.Revoke(revokedByIp, reasonRevoked, replacedByToken);
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Belirli bir kullanıcıya ait tüm refresh token'ları getirir.
    /// </summary>
    /// <param name="userId">Kullanıcı ID'si.</param>
    /// <returns>Kullanıcıya ait tüm refresh token'lar.</returns>
    public async Task<IEnumerable<RefreshToken<TId, TUserId>>> GetAllRefreshTokensAsync(TUserId userId)
    {
        return await Task.FromResult(_refreshTokens.Where(rt => rt.UserId.Equals(userId)).ToList());
    }

    /// <summary>
    /// Süresi dolmuş refresh token'ları depodan kaldırır.
    /// </summary>
    public async Task RemoveExpiredRefreshTokensAsync()
    {
        var expiredTokens = _refreshTokens.Where(rt => rt.ExpirationDate < DateTime.UtcNow).ToList();
        foreach (var token in expiredTokens)
        {
            _refreshTokens.Remove(token);
        }
        await Task.CompletedTask;
    }
}