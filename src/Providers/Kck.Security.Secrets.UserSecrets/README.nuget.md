# Kck.Security.Secrets.UserSecrets

.NET User Secrets-backed `ISecretsManager` for local development secret management without committing credentials to source control.

## Installation

```bash
dotnet add package Kck.Security.Secrets.UserSecrets
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckUserSecrets();

// Initialize user secrets in project (one-time)
// dotnet user-secrets init
// dotnet user-secrets set "Database:Password" "dev-password"

// Inject and use
public class DevSetupService(ISecretsManager secrets)
{
    public async Task<string> GetDbPasswordAsync(CancellationToken ct)
        => await secrets.GetSecretAsync("Database:Password", ct);
}
```

## Configuration

User secrets are stored in the OS user profile directory and are loaded automatically by `AddKckUserSecrets()` in development environments.

| Property | Description | Default |
|---|---|---|
| `UserSecretsId` | Project user secrets ID | from `.csproj` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
