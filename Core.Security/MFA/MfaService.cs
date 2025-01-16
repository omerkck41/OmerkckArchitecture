using System.Security.Cryptography;

namespace Core.Security.MFA;

public class MfaService : IMfaService
{
    private readonly TotpService _totpService;

    public MfaService(TotpService totpService)
    {
        _totpService = totpService;
    }

    public async Task<string> GenerateAuthenticatorCodeAsync()
    {
        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

        return await Task.FromResult(code);
    }

    public async Task<bool> ValidateAuthenticatorCodeAsync(string inputCode, string expectedCode)
    {
        var isValid = inputCode == expectedCode;

        return await Task.FromResult(isValid);
    }

    public async Task<string> GenerateTotpCodeAsync(string secretKey)
    {
        return await TotpService.GenerateTotpCodeAsync(secretKey);
    }

    public async Task<bool> ValidateTotpCodeAsync(string inputCode, string secretKey)
    {
        return await _totpService.ValidateTotpCodeAsync(inputCode, secretKey);
    }
}