namespace Core.Security.MFA;

public interface ITotpService
{
    Task<string> GenerateTotpCodeAsync(string secretKey, DateTimeOffset? timestamp = null);
    Task<bool> ValidateTotpCodeAsync(string inputCode, string secretKey);
}