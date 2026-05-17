namespace Kck.Security.Secrets.AzureKeyVault;

/// <summary>
/// Configuration options for the Azure Key Vault secrets manager.
/// </summary>
public sealed class AzureKeyVaultOptions
{
    /// <summary>Gets or sets the full URI of the Azure Key Vault instance (e.g. <c>https://my-vault.vault.azure.net/</c>).</summary>
    public required string VaultUri { get; set; }

    /// <summary>Gets or sets an optional prefix prepended to every secret name to support multi-tenant or multi-environment vaults.</summary>
    public string? SecretPrefix { get; set; }
}
