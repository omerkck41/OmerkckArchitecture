using Microsoft.Extensions.Options;

namespace Kck.EventBus.AzureServiceBus.DependencyInjection;

internal sealed class AzureServiceBusOptionsValidator : IValidateOptions<AzureServiceBusOptions>
{
    public ValidateOptionsResult Validate(string? name, AzureServiceBusOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            errors.Add(
                """
                  • ConnectionString: boş veya null
                    → Fix: opt.ConnectionString = "Endpoint=sb://...";
                    → Alternatif: Azure Managed Identity kullanıyorsanız bağlantı dizesi yerine
                      namespace endpoint: "sb://<namespace>.servicebus.windows.net/"
                """);

        if (string.IsNullOrWhiteSpace(options.TopicName))
            errors.Add(
                """
                  • TopicName: boş veya null
                    → Fix: opt.TopicName = "kck-eventbus"
                """);

        if (string.IsNullOrWhiteSpace(options.SubscriptionName))
            errors.Add(
                """
                  • SubscriptionName: boş veya null
                    → Fix: opt.SubscriptionName = "my-service"
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.EventBus.AzureServiceBus] AzureServiceBusOptions geçersiz:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/event-bus.md
            """);
    }
}
