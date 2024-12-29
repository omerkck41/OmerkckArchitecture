namespace Core.Security.TokenRevocation;

public class TokenBlacklist
{
    private readonly HashSet<string> _revokedTokens = new();

    public void RevokeToken(string token)
    {
        _revokedTokens.Add(token);
    }

    public bool IsTokenRevoked(string token)
    {
        return _revokedTokens.Contains(token);
    }
}