namespace Kck.Security.Abstractions.Token;

/// <summary>
/// Manages token revocation. Implementations should provide O(1) lookups.
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>Revokes a token with an expiration matching the token's remaining lifetime.</summary>
    Task RevokeAsync(string tokenId, TimeSpan expiration, CancellationToken ct = default);

    /// <summary>Checks if a token has been revoked.</summary>
    Task<bool> IsRevokedAsync(string tokenId, CancellationToken ct = default);
}
