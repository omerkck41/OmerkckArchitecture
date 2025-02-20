using OtpNet;
using System.Web;

namespace Core.Security.MFA;

public abstract class BaseOtpService : IOtpService
{
    protected abstract OtpHashMode HashAlgorithm { get; }
    protected abstract int OtpSize { get; }

    public async Task<string> GenerateSecretKey()
    {
        return await Task.FromResult(Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20)));
    }

    public abstract Task<string> GenerateOtpCodeAsync(string secretKey);
    public abstract Task<bool> ValidateOtpCodeAsync(string secretKey, string otp);

    public async Task<string> GenerateOtpAuthUrlAsync(string account, string issuer, string secretKey)
    {
        string encodedIssuer = HttpUtility.UrlEncode(issuer);
        string encodedAccount = HttpUtility.UrlEncode(account);
        return await Task.FromResult($"otpauth://totp/{encodedIssuer}:{encodedAccount}?secret={secretKey}&issuer={encodedIssuer}&algorithm={HashAlgorithm}&digits={OtpSize}&period=30");
    }
}