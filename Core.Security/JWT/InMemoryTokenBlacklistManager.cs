using System.Collections.Concurrent;

namespace Core.Security.JWT;

public class InMemoryTokenBlacklistManager : ITokenBlacklistManager
{
    private static readonly ConcurrentDictionary<string, DateTime> Blacklist = new();

    public void RevokeToken(string token, TimeSpan expiration)
    {
        Blacklist[token] = DateTime.UtcNow.Add(expiration);
    }

    public bool IsTokenRevoked(string token)
    {
        return Blacklist.TryGetValue(token, out var expiration) && expiration > DateTime.UtcNow;
    }
}