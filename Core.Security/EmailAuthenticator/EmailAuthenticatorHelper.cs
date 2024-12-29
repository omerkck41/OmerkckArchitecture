using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Core.Security.EmailAuthenticator;

public class EmailAuthenticatorHelper : IEmailAuthenticatorHelper
{
    private readonly ILogger<EmailAuthenticatorHelper> _logger;

    public EmailAuthenticatorHelper(ILogger<EmailAuthenticatorHelper> logger)
    {
        _logger = logger;
    }

    public async Task<string> CreateEmailActivationKeyAsync()
    {
        try
        {
            var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            _logger.LogInformation("Generated Email Activation Key.");
            return await Task.FromResult(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate email activation key.");
            throw;
        }
    }

    public async Task<string> CreateEmailActivationCodeAsync()
    {
        try
        {
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            _logger.LogInformation("Generated Email Activation Code: {Code}", code);
            return await Task.FromResult(code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate email activation code.");
            throw;
        }
    }

    public async Task<bool> ValidateActivationCodeAsync(string inputCode, string expectedCode)
    {
        try
        {
            var isValid = inputCode == expectedCode;
            _logger.LogInformation("Activation Code Validation Result: {IsValid}", isValid);
            return await Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate activation code.");
            throw;
        }
    }
}