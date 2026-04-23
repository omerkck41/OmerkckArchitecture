namespace Kck.Security.Secrets.AzureKeyVault;

public sealed class AzureKeyVaultOptions
{
    public required string VaultUri { get; set; }
    public string? SecretPrefix { get; set; }
}
