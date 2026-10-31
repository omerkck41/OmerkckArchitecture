using Microsoft.Extensions.Options;

namespace Kck.FileStorage.AzureBlob.DependencyInjection;

/// <summary>Validates <see cref="AzureBlobOptions"/> at application startup.</summary>
public sealed class AzureBlobOptionsValidator : IValidateOptions<AzureBlobOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, AzureBlobOptions options)
    {
        var errors = new List<string>();

        var hasCs = !string.IsNullOrWhiteSpace(options.ConnectionString);
        var hasAccount = !string.IsNullOrWhiteSpace(options.AccountName);

        if (!hasCs && !hasAccount)
            errors.Add(
                """
                  • ConnectionString and AccountName are both empty
                    → Fix: opt.ConnectionString = "<connection-string>";
                    → or, for Managed Identity: opt.AccountName = "<account-name>";
                """);

        if (hasCs && hasAccount)
            errors.Add(
                """
                  • ConnectionString and AccountName are both set — use only one
                """);

        if (string.IsNullOrWhiteSpace(options.ContainerName))
            errors.Add(
                """
                  • ContainerName: null or empty
                    → Fix: opt.ContainerName = "my-container";
                """);

        if (errors.Count == 0)
            return ValidateOptionsResult.Success;

        return ValidateOptionsResult.Fail(
            $"""
            [Kck.FileStorage.AzureBlob] AzureBlobOptions is invalid:
            {string.Join(Environment.NewLine, errors)}
              Docs: https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/file-storage.md
            """);
    }
}
