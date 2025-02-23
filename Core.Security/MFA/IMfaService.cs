namespace Core.Security.MFA;

public interface IMfaService
{
    // Authenticator (6 haneli rastgele kod) üretir.
    Task<string> GenerateAuthenticatorCodeAsync();


    // Authenticator kodunu doğrular.
    Task<bool> ValidateAuthenticatorCodeAsync(string inputCode, string expectedCode);

    // TOTP (Time-Based One-Time Password) (6 haneli SecretKey) üretir.
    Task<string> GenerateOtpSecretKey();
    // TOTP (Time-Based One-Time Password) üretir.
    Task<string> GenerateOtpCodeAsync(string secretKey);
    // TOTP for Google
    Task<string> GenerateOtpAuthUrlAsync(string account, string issuer, string secretKey);
    // TOTP kodunu doğrular.
    Task<bool> ValidateOtpCodeAsync(string inputCode, string secretKey);


    // E-posta ile gönderilecek 6 haneli rastgele kod üretir.
    Task<string> GenerateEmailCodeAsync();
    // E-posta kodunu doğrular.
    Task<bool> ValidateEmailCodeAsync(string inputCode, string expectedCode);


    // SMS ile gönderilecek 6 haneli rastgele kod üretir.
    Task<string> GenerateSmsCodeAsync();
    // SMS kodunu doğrular.
    Task<bool> ValidateSmsCodeAsync(string inputCode, string expectedCode);
}