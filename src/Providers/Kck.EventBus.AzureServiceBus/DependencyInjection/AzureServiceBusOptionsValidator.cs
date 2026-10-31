using Microsoft.Extensions.Options;

namespace Kck.EventBus.AzureServiceBus.DependencyInjection;

public sealed class AzureServiceBusOptionsValidator : IValidateOptions<AzureServiceBusOptions>
{
    public ValidateOptionsResult Validate(string? name, AzureServiceBusOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            errors.Add(
                """
                  • ConnectionString: null or empty
                    → Fix: opt.ConnectionString = "Endpoint=sb://...";
                    → Alternative: with Azure Managed Identity, use the namespace endpoint instead of a connection string:
                      "sb://<namespace>.servicebus.windows.net/"
                """);

        if (string.IsNullOrWhiteSpace(options.TopicName))
            errors.Add(
                """
                  • TopicName: null or empty
                    → Fix: opt.TopicName = "kck-eventbus"
                """);

        if (string.IsNullOrWhiteSpace(options.SubscriptionName))
            errors.Add(
                """
                  • SubscriptionName: null or empty
                    → Fix: opt.SubscriptionName = "my-service"
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.EventBus.AzureServiceBus] AzureServiceBusOptions is invalid:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/event-bus.md
            """);
    }
}
