using Core.Persistence.Entities;

namespace Core.Security.JWT;

/// <summary>
/// İptal edilen token'lar için bir kara liste temsil eder.
/// </summary>
public class TokenBlacklist<TId, TUserId> : Entity<TId>
{
    public TUserId UserId { get; set; }

    public string Token { get; set; }
    public DateTime RevokedDate { get; set; }
    public string? Reason { get; set; }


    public TokenBlacklist() { UserId = default!; Token = string.Empty; Reason = string.Empty; }

    public TokenBlacklist(TId id, TUserId userId, string token, string? reason = null) : base(id)
    {
        UserId = userId;
        Token = token;
        RevokedDate = DateTime.UtcNow;
        Reason = reason;
    }

}