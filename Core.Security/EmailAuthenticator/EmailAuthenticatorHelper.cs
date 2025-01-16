using System.Security.Cryptography;

namespace Core.Security.EmailAuthenticator;

public class EmailAuthenticatorHelper : IEmailAuthenticatorHelper
{
    public async Task<string> CreateEmailActivationKeyAsync()
    {
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return await Task.FromResult(key);
    }

    public async Task<string> CreateEmailActivationCodeAsync()
    {
        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        return await Task.FromResult(code);
    }

    public async Task<bool> ValidateActivationCodeAsync(string inputCode, string expectedCode)
    {
        var isValid = inputCode == expectedCode;
        return await Task.FromResult(isValid);
    }
}