namespace Core.Security.MFA;

public interface IMfaService
{
    Task<string> GenerateAuthenticatorCodeAsync();
    Task<bool> ValidateAuthenticatorCodeAsync(string inputCode, string expectedCode);
    Task<string> GenerateTotpCodeAsync(string secretKey);
    Task<bool> ValidateTotpCodeAsync(string inputCode, string secretKey);
}