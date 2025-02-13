using System.Collections.Concurrent;

namespace Core.Security.JWT;

public class InMemoryTokenBlacklistManager<TUserId> : ITokenBlacklistManager<TUserId>
{
    private static readonly ConcurrentDictionary<string, DateTime> Blacklist = new();
    private const string BlacklistKeyPrefix = "user_token_";

    public void RevokeToken(string token, TUserId userId, TimeSpan expiration)
    {
        Blacklist[$"{BlacklistKeyPrefix}{userId}_{token}"] = DateTime.UtcNow.Add(expiration);
    }

    public bool IsTokenRevoked(string token)
    {
        return Blacklist.Keys.Any(key => key.EndsWith(token) && Blacklist[key] > DateTime.UtcNow);
    }

    public bool IsUserRevoked(TUserId userId)
    {
        return Blacklist.Keys.Any(key => key.StartsWith($"{BlacklistKeyPrefix}{userId}_") && Blacklist[key] > DateTime.UtcNow);
    }

    public void RemoveFromBlacklist(string token)
    {
        Blacklist.TryRemove($"token_{token}", out _);
    }

    public void RemoveUserFromBlacklist(TUserId userId)
    {
        var keysToRemove = Blacklist.Keys
        .Where(key => key.StartsWith($"user_token_{userId}_"))
        .ToList();

        foreach (var key in keysToRemove)
        {
            Blacklist.TryRemove(key, out _);
        }
    }
}