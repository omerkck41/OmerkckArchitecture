using System.Collections.Concurrent;

namespace Core.Security.JWT;

public class InMemoryTokenBlacklistManager<TUserId> : ITokenBlacklistManager<TUserId>
{
    private static readonly ConcurrentDictionary<string, DateTime> Blacklist = new();

    public void RevokeToken(string token, TUserId userId, TimeSpan expiration)
    {
        var key = TokenKeyHelper.BuildKey(userId, token);
        Blacklist[key] = DateTime.UtcNow.Add(expiration);
    }

    public bool IsTokenRevoked(string token)
    {
        // Token'a ait tüm key'leri kontrol ediyoruz.
        return Blacklist.Keys.Any(key => key.EndsWith("_" + token) && Blacklist[key] > DateTime.UtcNow);
    }

    public bool IsUserRevoked(TUserId userId)
    {
        return Blacklist.Keys.Any(key => key.StartsWith($"user_token_{userId}_") && Blacklist[key] > DateTime.UtcNow);
    }

    public void RemoveFromBlacklist(string token)
    {
        // Token'a ait tüm key'leri sil
        var keysToRemove = Blacklist.Keys.Where(key => key.EndsWith("_" + token)).ToList();
        foreach (var key in keysToRemove)
        {
            Blacklist.TryRemove(key, out _);
        }
    }

    public void RemoveUserFromBlacklist(TUserId userId)
    {
        var keysToRemove = Blacklist.Keys.Where(key => key.StartsWith($"user_token_{userId}_")).ToList();
        foreach (var key in keysToRemove)
        {
            Blacklist.TryRemove(key, out _);
        }
    }
}