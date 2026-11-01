using Amazon.SimpleEmailV2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kck.Messaging.AmazonSes.DependencyInjection;

/// <summary>
/// Validates <see cref="AmazonSesOptions"/>: <c>Region</c> is required unless an
/// <see cref="IAmazonSimpleEmailServiceV2"/> is registered, which the provider then uses instead; <c>AccessKey</c> and
/// <c>SecretKey</c> are set together or not at all.
/// </summary>
public sealed class AmazonSesOptionsValidator : IValidateOptions<AmazonSesOptions>
{
    private readonly IServiceProviderIsService? _isService;

    /// <summary>Creates a validator that requires <c>Region</c>.</summary>
    public AmazonSesOptionsValidator()
        : this(null)
    {
    }

    /// <summary>
    /// Creates a validator that accepts an empty <c>Region</c> when <paramref name="isService"/> reports a registered
    /// <see cref="IAmazonSimpleEmailServiceV2"/>.
    /// </summary>
    /// <param name="isService">The container's service lookup; <see langword="null"/> requires <c>Region</c>.</param>
    public AmazonSesOptionsValidator(IServiceProviderIsService? isService)
    {
        _isService = isService;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, AmazonSesOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Region)
            && _isService?.IsService(typeof(IAmazonSimpleEmailServiceV2)) != true)
            errors.Add(
                """
                  • Region: null or empty and no IAmazonSimpleEmailServiceV2 is registered
                    → Fix: opt.Region = "eu-central-1"
                    → or register your own IAmazonSimpleEmailServiceV2 in the container
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
