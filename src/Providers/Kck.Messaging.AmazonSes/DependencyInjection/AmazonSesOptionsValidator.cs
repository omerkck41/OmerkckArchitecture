using Microsoft.Extensions.Options;

namespace Kck.Messaging.AmazonSes.DependencyInjection;

public sealed class AmazonSesOptionsValidator : IValidateOptions<AmazonSesOptions>
{
    public ValidateOptionsResult Validate(string? name, AmazonSesOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Region))
            errors.Add(
                """
                  • Region: null or empty
                    → Fix: opt.Region = "eu-central-1"
                """);

        // Explicit credentials optional (IAM role / env vars fallback is supported).
        // But if one is set, the other is required.
        var hasAccessKey = !string.IsNullOrWhiteSpace(options.AccessKey);
        var hasSecretKey = !string.IsNullOrWhiteSpace(options.SecretKey);
        if (hasAccessKey != hasSecretKey)
            errors.Add(
                """
                  • AccessKey / SecretKey: set both or leave both empty
                    → Fix: opt.AccessKey = "AKIA...";  opt.SecretKey = "...";
                    → Alternative: use an IAM role or environment variables (AWS_ACCESS_KEY_ID)
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.Messaging.AmazonSes] AmazonSesOptions is invalid:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md
            """);
    }
}
