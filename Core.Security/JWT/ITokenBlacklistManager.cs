namespace Core.Security.JWT;

public interface ITokenBlacklistManager
{
    void RevokeToken(string token, string userId, TimeSpan expiration);
    bool IsTokenRevoked(string token);
    bool IsUserRevoked(string userId);
    void RemoveFromBlacklist(string token);
    void RemoveUserFromBlacklist(string userId);
}