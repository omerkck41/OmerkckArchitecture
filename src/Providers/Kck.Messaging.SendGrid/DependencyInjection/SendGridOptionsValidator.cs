using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SendGrid;

namespace Kck.Messaging.SendGrid.DependencyInjection;

/// <summary>
/// Validates <see cref="SendGridOptions"/>: <c>ApiKey</c> is required unless an <see cref="ISendGridClient"/> is
/// registered, which the provider then uses instead.
/// </summary>
public sealed class SendGridOptionsValidator : IValidateOptions<SendGridOptions>
{
    private readonly IServiceProviderIsService? _isService;

    /// <summary>Creates a validator that requires <c>ApiKey</c>.</summary>
    public SendGridOptionsValidator()
        : this(null)
    {
    }

    /// <summary>
    /// Creates a validator that accepts an empty <c>ApiKey</c> when <paramref name="isService"/> reports a registered
    /// <see cref="ISendGridClient"/>.
    /// </summary>
    /// <param name="isService">The container's service lookup; <see langword="null"/> requires <c>ApiKey</c>.</param>
    public SendGridOptionsValidator(IServiceProviderIsService? isService)
    {
        _isService = isService;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SendGridOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!string.IsNullOrWhiteSpace(options.ApiKey)
            || _isService?.IsService(typeof(ISendGridClient)) == true)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            """
            [Kck.Messaging.SendGrid] SendGridOptions is invalid:
              • ApiKey: null or empty and no ISendGridClient is registered
                → Fix: opt.ApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")!
                → or register your own ISendGridClient in the container
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/messaging.md
            """);
    }
}
