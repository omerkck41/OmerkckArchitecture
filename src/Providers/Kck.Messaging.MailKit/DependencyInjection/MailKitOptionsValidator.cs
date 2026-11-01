using MailKit.Security;
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

        // An explicit None never negotiates TLS: authenticating would send the credentials in plaintext.
        if (options.SecureSocketOptions == SecureSocketOptions.None && !string.IsNullOrWhiteSpace(options.UserName))
            errors.Add(
                """
                  • UserName is set with SecureSocketOptions = None: credentials would be sent in plaintext
                    → Fix: opt.SecureSocketOptions = SecureSocketOptions.StartTls (or SslOnConnect)
                    → or clear UserName for an unauthenticated relay
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
