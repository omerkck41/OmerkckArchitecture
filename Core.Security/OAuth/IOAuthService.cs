using System.Security.Claims;

namespace Core.Security.OAuth;

public interface IOAuthService
{
    string GetAuthorizationUrl();
    Task<string> ExchangeCodeForTokenAsync(string authorizationCode);
    Task<string> RefreshAccessTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    Task<string> GetUserInfoAsync(string token);
    bool IsTokenExpired(string token);
    IEnumerable<Claim> ParseTokenClaims(string token);
}