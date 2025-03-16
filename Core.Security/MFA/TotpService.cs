using Microsoft.Extensions.Options;
using OtpNet;

namespace Core.Security.MFA;

public class TotpService : BaseOtpService
{
    private readonly OtpSettings _otpSettings;

    public TotpService(IOptions<OtpSettings> otpSettings)
    {
        _otpSettings = otpSettings.Value ?? new OtpSettings();
    }

    protected override OtpHashMode HashAlgorithm => OtpHashMode.Sha256;
    protected override int OtpSize => _otpSettings.OtpLength;
    protected override int OtpPeriod => _otpSettings.OtpExpirySeconds;



    public override Task<string> GenerateOtpCodeAsync(string secretKey)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey),
            step: _otpSettings.OtpExpirySeconds,
            totpSize: OtpSize,
            mode: HashAlgorithm);

        return Task.FromResult(totp.ComputeTotp());
    }

    public override Task<bool> ValidateOtpCodeAsync(string secretKey, string otp)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey),
            step: _otpSettings.OtpExpirySeconds,
            totpSize: OtpSize,
            mode: HashAlgorithm);

        return Task.FromResult(totp.VerifyTotp(otp, out _, new VerificationWindow(1, 1)));
    }
}

public class OtpSettings
{
    public int OtpLength { get; set; } = 6;
    public int OtpExpirySeconds { get; set; } = 60;
}