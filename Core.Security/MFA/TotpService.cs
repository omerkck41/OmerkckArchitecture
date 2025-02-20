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



    public override async Task<string> GenerateOtpCodeAsync(string secretKey)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey), step: _otpSettings.OtpExpirySeconds, totpSize: OtpSize, mode: HashAlgorithm);
        return totp.ComputeTotp();
    }

    public override async Task<bool> ValidateOtpCodeAsync(string secretKey, string otp)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey), step: _otpSettings.OtpExpirySeconds, totpSize: OtpSize, mode: HashAlgorithm);
        return totp.VerifyTotp(otp, out _);
    }
}

public class OtpSettings
{
    public int OtpLength { get; set; } = 6;
    public int OtpExpirySeconds { get; set; } = 60;
}