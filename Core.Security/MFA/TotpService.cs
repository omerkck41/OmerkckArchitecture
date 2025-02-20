using OtpNet;

namespace Core.Security.MFA;

public class TotpService : BaseOtpService
{
    protected override OtpHashMode HashAlgorithm => OtpHashMode.Sha256;
    protected override int OtpSize => 6;



    public override async Task<string> GenerateOtpCodeAsync(string secretKey)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey), step: 30, totpSize: OtpSize, mode: HashAlgorithm);
        return totp.ComputeTotp();
    }

    public override async Task<bool> ValidateOtpCodeAsync(string secretKey, string otp)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secretKey), step: 30, totpSize: OtpSize, mode: HashAlgorithm);
        return totp.VerifyTotp(otp, out _);
    }
}