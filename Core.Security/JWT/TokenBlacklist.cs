using Core.Persistence.Entities;

namespace Core.Security.JWT;

/// <summary>
/// İptal edilen token'lar için bir kara liste temsil eder.
/// </summary>
public class TokenBlacklist<TId> : Entity<TId>
{
    public string Token { get; set; }  // İptal edilen token
    public DateTime RevokedDate { get; set; }  // Token'ın iptal edildiği tarih
    public string? Reason { get; set; }  // İptal sebebi (Opsiyonel)

    public TokenBlacklist() { Token = string.Empty; RevokedDate = DateTime.UtcNow; Reason = string.Empty; }

    public TokenBlacklist(TId id, string token, string? reason = null) : base(id)
    {
        Token = token;
        RevokedDate = DateTime.UtcNow;
        Reason = reason;
    }

}