using System.Security.Cryptography;

namespace Core.Security.MFA;

public class MfaService : IMfaService
{
    private readonly TotpService _totpService;

    public MfaService(TotpService totpService)
    {
        _totpService = totpService;
    }

    // 6 haneli rastgele Authenticator kodu üretir.
    public async Task<string> GenerateAuthenticatorCodeAsync()
    {
        try
        {
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            return await Task.FromResult(code);
        }
        catch (Exception ex)
        {
            // Loglama yapılabilir.
            throw new InvalidOperationException("Failed to generate authenticator code.", ex);
        }
    }

    // Authenticator kodunu doğrular.
    public async Task<bool> ValidateAuthenticatorCodeAsync(string inputCode, string expectedCode)
    {
        try
        {
            var isValid = inputCode == expectedCode;
            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            // Loglama yapılabilir.
            throw new InvalidOperationException("Failed to validate authenticator code.", ex);
        }
    }

    // TOTP kodu üretir.
    public async Task<string> GenerateTotpCodeAsync(string secretKey)
    {
        return await _totpService.GenerateTotpCodeAsync(secretKey);
    }

    // TOTP kodunu doğrular.
    public async Task<bool> ValidateTotpCodeAsync(string inputCode, string secretKey)
    {
        return await _totpService.ValidateTotpCodeAsync(inputCode, secretKey);
    }

    // 6 haneli rastgele e-posta kodu üretir.
    public async Task<string> GenerateEmailCodeAsync()
    {
        try
        {
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            return await Task.FromResult(code);
        }
        catch (Exception ex)
        {
            // Loglama yapılabilir.
            throw new InvalidOperationException("Failed to generate email code.", ex);
        }
    }

    // E-posta kodunu doğrular.
    public async Task<bool> ValidateEmailCodeAsync(string inputCode, string expectedCode)
    {
        try
        {
            var isValid = inputCode == expectedCode;
            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            // Loglama yapılabilir.
            throw new InvalidOperationException("Failed to validate email code.", ex);
        }
    }

    // 6 haneli rastgele SMS kodu üretir.
    public async Task<string> GenerateSmsCodeAsync()
    {
        try
        {
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            return await Task.FromResult(code);
        }
        catch (Exception ex)
        {
            // Loglama yapılabilir.
            throw new InvalidOperationException("Failed to generate SMS code.", ex);
        }
    }

    // SMS kodunu doğrular.
    public async Task<bool> ValidateSmsCodeAsync(string inputCode, string expectedCode)
    {
        try
        {
            var isValid = inputCode == expectedCode;
            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            // Loglama yapılabilir.
            throw new InvalidOperationException("Failed to validate SMS code.", ex);
        }
    }
}