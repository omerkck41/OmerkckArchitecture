namespace Kck.Security.Abstractions.Mfa;

/// <summary>
/// Multi-factor authentication provider (TOTP, Email, SMS).
/// </summary>
public interface IMfaProvider
{
    /// <summary>Generates a new secret key for TOTP setup.</summary>
    Task<MfaSetupResult> GenerateSetupAsync(string accountName, string issuer, CancellationToken ct = default);

    /// <summary>Validates a one-time code against the secret key.</summary>
    Task<bool> ValidateCodeAsync(string secretKey, string code, CancellationToken ct = default);
}
