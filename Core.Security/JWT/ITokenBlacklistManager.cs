namespace Core.Security.JWT;

public interface ITokenBlacklistManager
{
    void RevokeToken(string token, TimeSpan expiration);
    bool IsTokenRevoked(string token);
}