using Core.Persistence.Entities;

namespace Core.Security.JWT;

/// <summary>
/// Yeni bir erişim token'ı almak için kullanılan bir refresh token'ı temsil eder.
/// </summary>
/// <typeparam name="TId">Token'ın benzersiz kimlik türü.</typeparam>
/// <typeparam name="TUserId">Kullanıcının benzersiz kimlik türü.</typeparam>
public class RefreshToken<TId, TUserId> : Entity<TId>
{
    /// <summary>
    /// Refresh token'ın ilişkilendirildiği kullanıcı ID'sini alır.
    /// </summary>
    public TUserId UserId { get; set; }

    /// <summary>
    /// Refresh token string'ini alır.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Refresh token'ın son kullanma tarihini alır.
    /// </summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Refresh token'ı oluşturan IP adresini alır.
    /// </summary>
    public string CreatedByIp { get; set; }

    /// <summary>
    /// Refresh token'ın iptal edildiği tarihi alır veya ayarlar.
    /// </summary>
    public DateTime? RevokedDate { get; set; }

    /// <summary>
    /// Refresh token'ı iptal eden IP adresini alır veya ayarlar.
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// Bu refresh token'ın yerine geçen token'ı alır veya ayarlar.
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Refresh token'ın neden iptal edildiğini açıklayan nedeni alır veya ayarlar.
    /// </summary>
    public string? ReasonRevoked { get; set; }


    public RefreshToken()
    {
        UserId = default!;
        Token = string.Empty;
        CreatedByIp = string.Empty;
    }

    /// <summary>
    /// <see cref="RefreshToken{TId, TUserId}"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="id">Refresh token'ın benzersiz kimliği.</param>
    /// <param name="userId">Refresh token'ın ilişkilendirildiği kullanıcı ID'si.</param>
    /// <param name="token">Refresh token string'i.</param>
    /// <param name="expirationDate">Refresh token'ın son kullanma tarihi.</param>
    /// <param name="createdByIp">Refresh token'ı oluşturan IP adresi.</param>
    public RefreshToken(TId id, TUserId userId, string token, DateTime expirationDate, string createdByIp) : base(id)
    {
        UserId = userId;
        Token = token;
        ExpirationDate = expirationDate;
        CreatedByIp = createdByIp;
    }

    /// <summary>
    /// Refresh token'ı iptal eder.
    /// </summary>
    /// <param name="revokedByIp">Token'ı iptal eden IP adresi.</param>
    /// <param name="reasonRevoked">Token'ın iptal edilme nedeni.</param>
    /// <param name="replacedByToken">Bu token'ın yerine geçen token (opsiyonel).</param>
    public void Revoke(string revokedByIp, string reasonRevoked, string? replacedByToken = null)
    {
        RevokedDate = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReasonRevoked = reasonRevoked;
        ReplacedByToken = replacedByToken;
    }
}