using Microsoft.Extensions.Options;

namespace Kck.Messaging.SendGrid.DependencyInjection;

public sealed class SendGridOptionsValidator : IValidateOptions<SendGridOptions>
{
    public ValidateOptionsResult Validate(string? name, SendGridOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            """
            [Kck.Messaging.SendGrid] SendGridOptions geçersiz:
              • ApiKey: boş veya null
                → Fix: opt.ApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")!
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md
            """);
    }
}
