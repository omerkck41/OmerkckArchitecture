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
                [Kck.FeatureFlags.AzureAppConfig] AzureAppConfigOptions geçersiz:
                  • ConnectionString ve Endpoint ikisi de boş
                    → Fix: opt.ConnectionString = "<connection-string>";
                    → veya Managed Identity için: opt.Endpoint = "https://<store>.azconfig.io";
                  Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/feature-flags.md
                """);

        if (hasCs && hasEp)
            return ValidateOptionsResult.Fail(
                """
                [Kck.FeatureFlags.AzureAppConfig] AzureAppConfigOptions geçersiz:
                  • ConnectionString ve Endpoint ikisi birden tanımlı — yalnızca biri kullanılmalı
                    → Managed Identity tercih ediliyorsa: opt.ConnectionString = null;
                """);

        if (options.RefreshInterval <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail(
                $"""
                [Kck.FeatureFlags.AzureAppConfig] AzureAppConfigOptions geçersiz:
                  • RefreshInterval: {options.RefreshInterval} (pozitif olmalı)
                    → Fix: opt.RefreshInterval = TimeSpan.FromSeconds(30);
                """);

        return ValidateOptionsResult.Success;
    }
}
