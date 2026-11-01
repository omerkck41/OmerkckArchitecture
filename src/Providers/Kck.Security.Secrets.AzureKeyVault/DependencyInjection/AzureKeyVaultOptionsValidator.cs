using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kck.Security.Secrets.AzureKeyVault.DependencyInjection;

/// <summary>
/// Validates <see cref="AzureKeyVaultOptions"/>: <c>VaultUri</c> must be an absolute HTTPS URI unless a
/// <see cref="SecretClient"/> is registered, which the secrets manager then uses instead.
/// </summary>
public sealed class AzureKeyVaultOptionsValidator : IValidateOptions<AzureKeyVaultOptions>
{
    private readonly IServiceProviderIsService? _isService;

    /// <summary>Creates a validator that requires <c>VaultUri</c>.</summary>
    public AzureKeyVaultOptionsValidator()
        : this(null)
    {
    }

    /// <summary>
    /// Creates a validator that skips <c>VaultUri</c> when <paramref name="isService"/> reports a registered
    /// <see cref="SecretClient"/>.
    /// </summary>
    /// <param name="isService">The container's service lookup; <see langword="null"/> requires <c>VaultUri</c>.</param>
    public AzureKeyVaultOptionsValidator(IServiceProviderIsService? isService)
    {
        _isService = isService;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, AzureKeyVaultOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // A registered SecretClient carries its own vault URI; VaultUri is not used then.
        if (_isService?.IsService(typeof(SecretClient)) == true)
            return ValidateOptionsResult.Success;

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.VaultUri))
        {
            errors.Add(
                """
                  • VaultUri: null or empty and no SecretClient is registered
                    → Fix: opt.VaultUri = "https://<vault-name>.vault.azure.net/"
                    → or register your own SecretClient in the container
                """);
        }
        else if (!Uri.TryCreate(options.VaultUri, UriKind.Absolute, out var uri)
                 || uri.Scheme != Uri.UriSchemeHttps)
        {
            errors.Add(
                $"""
                  • VaultUri: "{options.VaultUri}" is not a valid HTTPS URI
                    → Fix: opt.VaultUri = "https://<vault-name>.vault.azure.net/"
                """);
        }

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.Security.Secrets.AzureKeyVault] AzureKeyVaultOptions is invalid:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md
            """);
    }
}
