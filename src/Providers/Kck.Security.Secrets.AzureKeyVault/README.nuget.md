# Kck.Security.Secrets.AzureKeyVault

Azure Key Vault-backed `ISecretsManager` for reading application secrets with Managed Identity or connection string authentication.

## Installation

```bash
dotnet add package Kck.Security.Secrets.AzureKeyVault
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckAzureKeyVault(options =>
{
    options.VaultUri = "https://myvault.vault.azure.net/";
    options.SecretPrefix = "myapp--";
});

// Inject and use
public class DatabaseService(ISecretsManager secrets)
{
    public async Task<string> GetConnectionStringAsync(CancellationToken ct)
        => await secrets.GetSecretAsync("ConnectionStrings--Default", ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `VaultUri` | Azure Key Vault URI | required |
| `SecretPrefix` | Prefix filter for secret names | `""` |
| `CacheDuration` | How long secrets are cached locally | `TimeSpan.FromMinutes(10)` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
