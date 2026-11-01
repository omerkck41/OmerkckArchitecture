using Azure.Data.AppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kck.FeatureFlags.AzureAppConfig.DependencyInjection;

/// <summary>
/// Validates <see cref="AzureAppConfigOptions"/> at application startup: <c>ConnectionString</c> or <c>Endpoint</c> is
/// required unless a <see cref="ConfigurationClient"/> is registered, which the service then uses instead.
/// </summary>
public sealed class AzureAppConfigFeatureFlagOptionsValidator : IValidateOptions<AzureAppConfigOptions>
{
    private readonly IServiceProviderIsService? _isService;

    /// <summary>Creates a validator that requires <c>ConnectionString</c> or <c>Endpoint</c>.</summary>
    public AzureAppConfigFeatureFlagOptionsValidator()
        : this(null)
    {
    }

    /// <summary>
    /// Creates a validator that accepts empty <c>ConnectionString</c> and <c>Endpoint</c> when
    /// <paramref name="isService"/> reports a registered <see cref="ConfigurationClient"/>.
    /// </summary>
    /// <param name="isService">
    /// The container's service lookup; <see langword="null"/> requires <c>ConnectionString</c> or <c>Endpoint</c>.
    /// </param>
    public AzureAppConfigFeatureFlagOptionsValidator(IServiceProviderIsService? isService)
    {
        _isService = isService;
    }

    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, AzureAppConfigOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var hasCs = !string.IsNullOrWhiteSpace(options.ConnectionString);
        var hasEp = !string.IsNullOrWhiteSpace(options.Endpoint);

        if (!hasCs && !hasEp && _isService?.IsService(typeof(ConfigurationClient)) != true)
            return ValidateOptionsResult.Fail(
                """
                [Kck.FeatureFlags.AzureAppConfig] AzureAppConfigOptions is invalid:
                  • ConnectionString and Endpoint are both empty and no ConfigurationClient is registered
                    → Fix: opt.ConnectionString = "<connection-string>";
                    → or, for Managed Identity: opt.Endpoint = "https://<store>.azconfig.io";
                    → or register your own ConfigurationClient in the container
                  Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/feature-flags.md
                """);

        if (hasCs && hasEp)
            return ValidateOptionsResult.Fail(
                """
                [Kck.FeatureFlags.AzureAppConfig] AzureAppConfigOptions is invalid:
                  • ConnectionString and Endpoint are both set — use only one
                    → If you prefer Managed Identity: opt.ConnectionString = null;
                """);

        if (options.RefreshInterval <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail(
                $"""
                [Kck.FeatureFlags.AzureAppConfig] AzureAppConfigOptions is invalid:
                  • RefreshInterval: {options.RefreshInterval} (must be positive)
                    → Fix: opt.RefreshInterval = TimeSpan.FromSeconds(30);
                """);

        return ValidateOptionsResult.Success;
    }
}
