using Core.Persistence.Entities;

namespace Core.Security.JWT;

/// <summary>
/// İptal edilen token'lar için bir kara liste temsil eder.
/// </summary>
public class TokenBlacklist<TId, TUserId> : Entity<TId>
{
    public TUserId UserId { get; } // Kullanıcı bazlı sorgulama için UserId eklendi

    public string Token { get; set; }  // İptal edilen token
    public DateTime RevokedDate { get; set; }  // Token'ın iptal edildiği tarih
    public string? Reason { get; set; }  // İptal sebebi (Opsiyonel)

    public TokenBlacklist() { UserId = default!; Token = string.Empty; Reason = string.Empty; }

    public TokenBlacklist(TId id, TUserId userId, string token, string? reason = null) : base(id)
    {
        UserId = userId;
        Token = token;
        RevokedDate = DateTime.UtcNow;
        Reason = reason;
    }

}