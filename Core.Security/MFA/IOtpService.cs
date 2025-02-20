namespace Core.Security.MFA;

public interface IOtpService
{
    Task<string> GenerateSecretKey();
    Task<string> GenerateOtpCodeAsync(string secretKey);
    Task<bool> ValidateOtpCodeAsync(string secretKey, string otp);
    Task<string> GenerateOtpAuthUrlAsync(string account, string issuer, string secretKey);
}