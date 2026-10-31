using Microsoft.Extensions.Options;

namespace Kck.Security.Secrets.AzureKeyVault.DependencyInjection;

public sealed class AzureKeyVaultOptionsValidator : IValidateOptions<AzureKeyVaultOptions>
{
    public ValidateOptionsResult Validate(string? name, AzureKeyVaultOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.VaultUri))
        {
            errors.Add(
                """
                  • VaultUri: null or empty
                    → Fix: opt.VaultUri = "https://<vault-name>.vault.azure.net/"
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
