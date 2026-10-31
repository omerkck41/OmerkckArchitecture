using Microsoft.Extensions.Options;

namespace Kck.FeatureFlags.AzureAppConfig.DependencyInjection;

/// <summary>
/// Validates <see cref="AzureAppConfigOptions"/> at application startup.
/// </summary>
public sealed class AzureAppConfigFeatureFlagOptionsValidator : IValidateOptions<AzureAppConfigOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, AzureAppConfigOptions options)
    {
        var hasCs = !string.IsNullOrWhiteSpace(options.ConnectionString);
        var hasEp = !string.IsNullOrWhiteSpace(options.Endpoint);

        if (!hasCs && !hasEp)
            return ValidateOptionsResult.Fail(
                """
                [Kck.FeatureFlags.AzureAppConfig] AzureAppConfigOptions is invalid:
                  • ConnectionString and Endpoint are both empty
                    → Fix: opt.ConnectionString = "<connection-string>";
                    → or, for Managed Identity: opt.Endpoint = "https://<store>.azconfig.io";
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
