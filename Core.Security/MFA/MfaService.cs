using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Core.Security.MFA;

public class MfaService : IMfaService
{
    private readonly ILogger<MfaService> _logger;
    private readonly TotpService _totpService;

    public MfaService(ILogger<MfaService> logger, TotpService totpService)
    {
        _logger = logger;
        _totpService = totpService;
    }

    public async Task<string> GenerateAuthenticatorCodeAsync()
    {
        try
        {
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            _logger.LogInformation("Generated Authenticator Code: {Code}", code);
            return await Task.FromResult(code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate authenticator code.");
            throw;
        }
    }

    public async Task<bool> ValidateAuthenticatorCodeAsync(string inputCode, string expectedCode)
    {
        try
        {
            var isValid = inputCode == expectedCode;
            _logger.LogInformation("Authenticator Code Validation: {IsValid}", isValid);
            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate authenticator code.");
            throw;
        }
    }

    public async Task<string> GenerateTotpCodeAsync(string secretKey)
    {
        return await _totpService.GenerateTotpCodeAsync(secretKey);
    }

    public async Task<bool> ValidateTotpCodeAsync(string inputCode, string secretKey)
    {
        return await _totpService.ValidateTotpCodeAsync(inputCode, secretKey);
    }
}