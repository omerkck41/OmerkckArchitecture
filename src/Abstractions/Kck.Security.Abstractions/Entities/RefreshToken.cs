using Kck.Core.Abstractions.Entities;

namespace Kck.Security.Abstractions.Entities;

/// <summary>
/// Refresh token entity for token rotation tracking.
/// </summary>
public class RefreshToken<TId, TUserId> : Entity<TId>
{
    public TUserId UserId { get; set; } = default!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? RevokeReason { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke(string revokedByIp, string reason, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        RevokeReason = reason;
        ReplacedByToken = replacedByToken;
    }
}
