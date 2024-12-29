namespace Core.Security.EmailAuthenticator;

public interface IEmailAuthenticatorHelper
{
    Task<string> CreateEmailActivationKeyAsync();
    Task<string> CreateEmailActivationCodeAsync();
    Task<bool> ValidateActivationCodeAsync(string inputCode, string expectedCode);
}