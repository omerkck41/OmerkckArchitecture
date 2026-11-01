using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kck.EventBus.AzureServiceBus.DependencyInjection;

/// <summary>
/// Validates <see cref="AzureServiceBusOptions"/>: <c>TopicName</c> and <c>SubscriptionName</c> are required, and
/// <c>ConnectionString</c> too unless a <see cref="ServiceBusClient"/> is registered, which the bus then uses instead.
/// </summary>
public sealed class AzureServiceBusOptionsValidator : IValidateOptions<AzureServiceBusOptions>
{
    private readonly IServiceProviderIsService? _isService;

    /// <summary>Creates a validator that requires <c>ConnectionString</c>.</summary>
    public AzureServiceBusOptionsValidator()
        : this(null)
    {
    }

    /// <summary>
    /// Creates a validator that accepts an empty <c>ConnectionString</c> when <paramref name="isService"/> reports a
    /// registered <see cref="ServiceBusClient"/>.
    /// </summary>
    /// <param name="isService">The container's service lookup; <see langword="null"/> requires <c>ConnectionString</c>.</param>
    public AzureServiceBusOptionsValidator(IServiceProviderIsService? isService)
    {
        _isService = isService;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, AzureServiceBusOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString)
            && _isService?.IsService(typeof(ServiceBusClient)) != true)
            errors.Add(
                """
                  • ConnectionString: null or empty and no ServiceBusClient is registered
                    → Fix: opt.ConnectionString = "Endpoint=sb://...";
                    → Alternative: with Azure Managed Identity, register your own client before UseAzureServiceBus:
                      services.AddSingleton(new ServiceBusClient("<namespace>.servicebus.windows.net", new DefaultAzureCredential()))
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
