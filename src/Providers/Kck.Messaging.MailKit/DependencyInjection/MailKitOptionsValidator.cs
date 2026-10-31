using Microsoft.Extensions.Options;

namespace Kck.Messaging.MailKit.DependencyInjection;

public sealed class MailKitOptionsValidator : IValidateOptions<MailKitOptions>
{
    public ValidateOptionsResult Validate(string? name, MailKitOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
            errors.Add(
                """
                  • Host: null or empty
                    → Fix: opt.Host = "smtp.example.com"
                """);

        if (options.Port is < 1 or > 65535)
            errors.Add(
                $"""
                  • Port: {options.Port} is invalid (must be between 1 and 65535)
                    → Fix: opt.Port = 587   (STARTTLS) or 465 (SSL)
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.Messaging.MailKit] MailKitOptions is invalid:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md
            """);
    }
}
