# Kck.Security.Secrets.UserSecrets

.NET User Secrets-backed `ISecretsManager` for local development secret management without committing credentials to source control.

## Installation

```bash
dotnet add package Kck.Security.Secrets.UserSecrets
```

## Quick Start

```csharp
using Kck.Security.Abstractions.Secrets;

// Program.cs - reads IConfiguration (user secrets, environment variables, appsettings); read-only
builder.Services.AddKckUserSecrets();

// Initialize user secrets in the project (one-time)
// dotnet user-secrets init
// dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;..."

// Inject and use
public class DatabaseService(ISecretsManager secrets)
{
    public async Task<string> GetConnectionStringAsync(CancellationToken ct) =>
        await secrets.GetSecretAsync("ConnectionStrings:Default", ct)
        ?? throw new InvalidOperationException("Secret ConnectionStrings:Default is not set.");
}
```

## Configuration

No options. `AddKckUserSecrets()` registers a read-only `ISecretsManager` over `IConfiguration` (user secrets, environment variables, appsettings); `SetSecretAsync` throws `NotSupportedException`.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
