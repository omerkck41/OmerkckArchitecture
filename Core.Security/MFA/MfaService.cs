using System.Security.Cryptography;

namespace Core.Security.MFA;

public class MfaService(IOtpService totpService) : IMfaService
{
    private readonly IOtpService _totpService = totpService;

    private static async Task<string> GenerateRandomCodeAsync()
    {
        return await Task.FromResult(RandomNumberGenerator.GetInt32(100000, 999999).ToString());
    }

    // 6 haneli rastgele Authenticator kodu üretir.
    public async Task<string> GenerateAuthenticatorCodeAsync() => await GenerateRandomCodeAsync();

    // Authenticator kodunu doğrular.
    public async Task<bool> ValidateAuthenticatorCodeAsync(string inputCode, string expectedCode) => await Task.FromResult(inputCode == expectedCode);

    // TOTP kodu üretir.
    public async Task<string> GenerateOtpCodeAsync(string secretKey) => await _totpService.GenerateOtpCodeAsync(secretKey);

    // TOTP kodunu doğrular.
    public async Task<bool> ValidateOtpCodeAsync(string inputCode, string secretKey) => await _totpService.ValidateOtpCodeAsync(inputCode, secretKey);

    // 6 haneli rastgele e-posta kodu üretir.
    public async Task<string> GenerateEmailCodeAsync() => await GenerateRandomCodeAsync();

    // E-posta kodunu doğrular.
    public async Task<bool> ValidateEmailCodeAsync(string inputCode, string expectedCode) => await Task.FromResult(inputCode == expectedCode);

    // 6 haneli rastgele SMS kodu üretir.
    public async Task<string> GenerateSmsCodeAsync() => await GenerateRandomCodeAsync();

    // SMS kodunu doğrular.
    public async Task<bool> ValidateSmsCodeAsync(string inputCode, string expectedCode) => await Task.FromResult(inputCode == expectedCode);
}