namespace Core.Security.JWT;

public interface ITokenBlacklistManager<TUserId>
{
    void RevokeToken(string token, TUserId userId, TimeSpan expiration);
    bool IsTokenRevoked(string token);
    bool IsUserRevoked(TUserId userId);
    void RemoveFromBlacklist(string token);
    void RemoveUserFromBlacklist(TUserId userId);
}