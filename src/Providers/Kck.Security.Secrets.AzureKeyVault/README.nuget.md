# Kck.Security.Secrets.AzureKeyVault

Azure Key Vault-backed `ISecretsManager` for reading application secrets with Managed Identity or connection string authentication.

## Installation

```bash
dotnet add package Kck.Security.Secrets.AzureKeyVault
```

## Quick Start

```csharp
using Kck.Security.Abstractions.Secrets;

// Program.cs - DefaultAzureCredential (Managed Identity, Azure CLI, environment)
builder.Services.AddKckAzureKeyVault(options =>
{
    options.VaultUri = "https://myvault.vault.azure.net/";
    options.SecretPrefix = "myapp";   // keys become "myapp-<key>"; Key Vault names allow letters, digits and dashes
});

// Inject and use
public class DatabaseService(ISecretsManager secrets)
{
    public async Task<string> GetConnectionStringAsync(CancellationToken ct) =>
        await secrets.GetSecretAsync("ConnectionStrings-Default", ct)
        ?? throw new InvalidOperationException("Secret ConnectionStrings-Default is not set.");
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `VaultUri` | Key Vault URI | required |
| `SecretPrefix` | Prefix: key `k` reads secret `{SecretPrefix}-k` | `null` |

Authentication uses `DefaultAzureCredential`. Key Vault secret names allow letters, digits and dashes.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
