using Microsoft.Extensions.Options;

namespace Kck.Messaging.MailKit.DependencyInjection;

internal sealed class MailKitOptionsValidator : IValidateOptions<MailKitOptions>
{
    public ValidateOptionsResult Validate(string? name, MailKitOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
            errors.Add(
                """
                  • Host: boş veya null
                    → Fix: opt.Host = "smtp.example.com"
                """);

        if (options.Port is < 1 or > 65535)
            errors.Add(
                $"""
                  • Port: {options.Port} geçersiz (1–65535 arası olmalı)
                    → Fix: opt.Port = 587   (STARTTLS) veya 465 (SSL)
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.Messaging.MailKit] MailKitOptions geçersiz:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md
            """);
    }
}
