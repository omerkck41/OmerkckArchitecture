namespace Kck.Security.Abstractions.Mfa;

/// <summary>Result of MFA setup containing the secret key and provisioning URI.</summary>
public sealed record MfaSetupResult
{
    /// <summary>Base32-encoded secret key for the authenticator app.</summary>
    public required string SecretKey { get; init; }

    /// <summary>otpauth:// URI for QR code generation.</summary>
    public required string ProvisioningUri { get; init; }
}
