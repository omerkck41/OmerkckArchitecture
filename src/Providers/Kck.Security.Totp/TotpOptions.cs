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

    /// <summary>
    /// Failed codes allowed per secret within <see cref="FailedAttemptWindow"/>; once reached, every code for that
    /// secret (a correct one too) is rejected until the window ends, and a successful code resets the count.
    /// Default: 0, no limit. The count lives in this process's memory cache, so each instance counts on its own.
    /// </summary>
    public int MaxFailedAttempts { get; set; }

    /// <summary>The period failed codes are counted in, from the first failure. Default: 5 minutes.</summary>
    public TimeSpan FailedAttemptWindow { get; set; } = TimeSpan.FromMinutes(5);
}
