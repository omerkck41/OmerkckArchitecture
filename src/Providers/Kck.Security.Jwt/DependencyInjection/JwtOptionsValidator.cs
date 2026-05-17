using Microsoft.Extensions.Options;

namespace Kck.Security.Jwt.DependencyInjection;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Issuer))
            errors.Add(
                """
                  • Issuer: boş veya null
                    → Fix: opt.Issuer = "https://myapp.example.com"
                """);

        if (string.IsNullOrWhiteSpace(options.Audience))
            errors.Add(
                """
                  • Audience: boş veya null
                    → Fix: opt.Audience = "api://myapp"
                """);

        if (options.AccessTokenExpiration <= TimeSpan.Zero)
            errors.Add(
                $"""
                  • AccessTokenExpiration: {options.AccessTokenExpiration} (pozitif bir değer olmalı)
                    → Fix: opt.AccessTokenExpiration = TimeSpan.FromMinutes(15)
                """);

        if (options.RefreshTokenTtlDays < 1)
            errors.Add(
                $"""
                  • RefreshTokenTtlDays: {options.RefreshTokenTtlDays} (en az 1 gün olmalı)
                    → Fix: opt.RefreshTokenTtlDays = 7
                """);

        // RSA anahtar kaynağına göre ilgili alan zorunlu
        switch (options.KeySource)
        {
            case RsaKeySource.File when string.IsNullOrWhiteSpace(options.RsaKeyPath):
                errors.Add(
                    """
                      • RsaKeyPath: KeySource=File seçildi ama RsaKeyPath boş
                        → Fix: opt.RsaKeyPath = "/secrets/rsa-private.pem"
                    """);
                break;
            case RsaKeySource.Configuration when string.IsNullOrWhiteSpace(options.RsaKeyBase64):
                errors.Add(
                    """
                      • RsaKeyBase64: KeySource=Configuration seçildi ama RsaKeyBase64 boş
                        → Fix: opt.RsaKeyBase64 = Environment.GetEnvironmentVariable("JWT_RSA_KEY_B64")!
                    """);
                break;
        }

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.Security.Jwt] JwtOptions geçersiz:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md
            """);
    }
}
