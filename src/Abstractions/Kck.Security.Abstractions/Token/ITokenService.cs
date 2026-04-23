namespace Kck.Security.Abstractions.Token;

/// <summary>
/// Creates and validates security tokens (JWT, etc.).
/// </summary>
public interface ITokenService
{
    /// <summary>Creates an access token for the given claims.</summary>
    Task<TokenResult> CreateAccessTokenAsync(TokenRequest request, CancellationToken ct = default);

    /// <summary>Creates a refresh token.</summary>
    Task<string> CreateRefreshTokenAsync(CancellationToken ct = default);

    /// <summary>Validates a token and returns its claims.</summary>
    Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Extracts claims from a token WITHOUT signature validation.
    /// Do NOT use for authorization decisions — use <see cref="ValidateTokenAsync"/> instead.
    /// </summary>
    [Obsolete("Use ValidateTokenAsync. GetClaimsFromToken does NOT verify signature — unsafe for authorization. For debug/logging only.", DiagnosticId = "KCK0001")]
    IReadOnlyDictionary<string, string> GetClaimsFromToken(string token);
}
