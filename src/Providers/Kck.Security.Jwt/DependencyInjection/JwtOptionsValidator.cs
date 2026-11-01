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
                  • Issuer: null or empty
                    → Fix: opt.Issuer = "https://myapp.example.com"
                """);

        if (string.IsNullOrWhiteSpace(options.Audience))
            errors.Add(
                """
                  • Audience: null or empty
                    → Fix: opt.Audience = "api://myapp"
                """);

        if (options.AccessTokenExpiration <= TimeSpan.Zero)
            errors.Add(
                $"""
                  • AccessTokenExpiration: {options.AccessTokenExpiration} (must be positive)
                    → Fix: opt.AccessTokenExpiration = TimeSpan.FromMinutes(15)
                """);

        if (options.RefreshTokenTtlDays < 1)
            errors.Add(
                $"""
                  • RefreshTokenTtlDays: {options.RefreshTokenTtlDays} (must be at least 1 day)
                    → Fix: opt.RefreshTokenTtlDays = 7
                """);

        // A number bound from configuration (for example "2", the removed SecretsManager slot) is not a defined value
        // and would otherwise fail only at the first token.
        if (!Enum.IsDefined(options.KeySource))
            errors.Add(
                $"""
                  • KeySource: {options.KeySource} is not a supported value (allowed: {string.Join(", ", Enum.GetNames<RsaKeySource>())})
                    → Fix: opt.KeySource = RsaKeySource.Configuration (or "KeySource": "Configuration" in appsettings)
                """);

        // The matching field is required for the selected RSA key source
        switch (options.KeySource)
        {
            case RsaKeySource.File when string.IsNullOrWhiteSpace(options.RsaKeyPath):
                errors.Add(
                    """
                      • RsaKeyPath: KeySource=File but RsaKeyPath is empty
                        → Fix: opt.RsaKeyPath = "/secrets/rsa-private.pem"
                    """);
                break;
            case RsaKeySource.Configuration when string.IsNullOrWhiteSpace(options.RsaKeyBase64):
                errors.Add(
                    """
                      • RsaKeyBase64: KeySource=Configuration but RsaKeyBase64 is empty
                        → Fix: opt.RsaKeyBase64 = Environment.GetEnvironmentVariable("JWT_RSA_KEY_B64")!
                    """);
                break;
        }

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.Security.Jwt] JwtOptions is invalid:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md
            """);
    }
}
