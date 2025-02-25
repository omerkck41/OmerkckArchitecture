using Core.CrossCuttingConcerns.GlobalException.Exceptions;

namespace Core.Security.JWT;

/// <summary>
/// Kimlik doğrulama ve yetkilendirme için kullanılan bir erişim token'ını temsil eder.
/// </summary>
public class AccessToken
{
    /// <summary>
    /// JWT token string'ini alır.
    /// </summary>
    public string Token { get; private set; }
    /// <summary>
    /// Token'ın son kullanma tarihini alır.
    /// </summary>
    public DateTime ExpirationDate { get; private set; }

    /// <summary>
    /// <see cref="AccessToken"/> sınıfının yeni bir örneğini başlatır.
    /// </summary>
    /// <param name="token">JWT token string'i.</param>
    /// <param name="expirationDate">Token'ın son kullanma tarihi.</param>
    /// <exception cref="CustomException">Token null ise fırlatılır.</exception>
    public AccessToken(string token, DateTime expirationDate)
    {
        Token = token ?? throw new CustomException(nameof(token));
        ExpirationDate = expirationDate;
    }
}