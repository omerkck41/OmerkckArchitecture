namespace Kck.Security.Totp;

/// <summary>TOTP configuration.</summary>
public sealed class TotpOptions
{
    /// <summary>Number of digits in the OTP code. Default: 6.</summary>
    public int CodeLength { get; set; } = 6;

    /// <summary>Time step in seconds. Default: 30.</summary>
    public int StepSeconds { get; set; } = 30;

    /// <summary>Verification window (allows clock skew). Default: 1 step before/after.</summary>
    public int VerificationWindow { get; set; } = 1;
}
