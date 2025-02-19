using System.Security.Cryptography;

namespace Core.Security.MFA;

public class MfaService : IMfaService
{
    private readonly ITotpService _totpService;

    public MfaService(ITotpService totpService)
    {
        _totpService = totpService;
    }

    private Task<string> GenerateRandomCodeAsync()
    {
        return Task.FromResult(RandomNumberGenerator.GetInt32(100000, 999999).ToString());
    }

    // 6 haneli rastgele Authenticator kodu üretir.
    public Task<string> GenerateAuthenticatorCodeAsync() => GenerateRandomCodeAsync();

    // Authenticator kodunu doğrular.
    public Task<bool> ValidateAuthenticatorCodeAsync(string inputCode, string expectedCode) => Task.FromResult(inputCode == expectedCode);

    // TOTP kodu üretir.
    public Task<string> GenerateTotpCodeAsync(string secretKey) => _totpService.GenerateTotpCodeAsync(secretKey);

    // TOTP kodunu doğrular.
    public Task<bool> ValidateTotpCodeAsync(string inputCode, string secretKey) => _totpService.ValidateTotpCodeAsync(inputCode, secretKey);

    // 6 haneli rastgele e-posta kodu üretir.
    public Task<string> GenerateEmailCodeAsync() => GenerateRandomCodeAsync();

    // E-posta kodunu doğrular.
    public Task<bool> ValidateEmailCodeAsync(string inputCode, string expectedCode) => Task.FromResult(inputCode == expectedCode);

    // 6 haneli rastgele SMS kodu üretir.
    public Task<string> GenerateSmsCodeAsync() => GenerateRandomCodeAsync();

    // SMS kodunu doğrular.
    public Task<bool> ValidateSmsCodeAsync(string inputCode, string expectedCode) => Task.FromResult(inputCode == expectedCode);
}