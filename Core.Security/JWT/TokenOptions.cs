using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.Security.JWT;

/// <summary>
/// JWT token'ları için yapılandırma seçeneklerini temsil eder.
/// </summary>
public class TokenOptions
{
    /// <summary>
    /// Token'ın hedef kitlesini (audience) alır veya ayarlar.
    /// </summary>
    public string Audience { get; set; }
    /// <summary>
    /// Token'ın yayıncısını (issuer) alır veya ayarlar.
    /// </summary>
    public string Issuer { get; set; }
    /// <summary>
    /// Erişim token'ının dakika cinsinden son kullanma süresini alır veya ayarlar.
    /// </summary>
    public int AccessTokenExpiration { get; set; }
    /// <summary>
    /// Token'ı imzalamak için kullanılan güvenlik anahtarını alır veya ayarlar.
    /// </summary>
    public string SecurityKey { get; set; }
    /// <summary>
    /// Refresh token'ın gün cinsinden yaşam süresini (TTL) alır veya ayarlar.
    /// </summary>
    public int RefreshTokenTTL { get; set; }

    /// <summary>
    /// <see cref="TokenOptions"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    public TokenOptions()
    {
        Audience = string.Empty;
        Issuer = string.Empty;
        SecurityKey = string.Empty;
    }

    /// <summary>
    /// Belirtilen değerlerle <see cref="TokenOptions"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="audience">Token'ın hedef kitlesi (audience).</param>
    /// <param name="issuer">Token'ın yayıncısı (issuer).</param>
    /// <param name="accessTokenExpiration">Erişim token'ının dakika cinsinden son kullanma süresi.</param>
    /// <param name="securityKey">Token'ı imzalamak için kullanılan güvenlik anahtarı.</param>
    /// <param name="refreshTokenTtl">Refresh token'ın gün cinsinden yaşam süresi (TTL).</param>
    /// <exception cref="CustomArgumentException">Gerekli parametrelerden herhangi biri null ise fırlatılır.</exception>
    public TokenOptions(string audience, string issuer, int accessTokenExpiration, string securityKey, int refreshTokenTtl)
    {
        Audience = audience ?? throw new CustomArgumentException(nameof(audience));
        Issuer = issuer ?? throw new CustomArgumentException(nameof(issuer));
        AccessTokenExpiration = accessTokenExpiration;
        SecurityKey = securityKey ?? throw new CustomArgumentException(nameof(securityKey));
        RefreshTokenTTL = refreshTokenTtl;
    }
}