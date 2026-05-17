# Kck.Security.Abstractions

Security abstractions — password hashing, encryption, JWT token management, token blacklist, secrets management, and MFA provider contracts.

## Installation

```bash
dotnet add package Kck.Security.Abstractions
```

## Quick Start

```csharp
// Program.cs — register concrete providers
builder.Services.AddKckArgon2Hashing();
builder.Services.AddKckJwtTokens(builder.Configuration);

// Password hashing
public class AuthService(IHashingService hasher, ITokenService tokens)
{
    public async Task<string> RegisterAsync(string password, CancellationToken ct)
    {
        // Hash before storing
        return await hasher.HashAsync(password, ct);
    }

    public async Task<bool> VerifyPasswordAsync(
        string password, string storedHash, CancellationToken ct)
    {
        return await hasher.VerifyAsync(password, storedHash, ct);
    }

    public async Task<TokenResult> IssueTokenAsync(
        ClaimsPrincipal principal, CancellationToken ct)
    {
        return await tokens.GenerateAsync(principal, ct);
    }
}

// Encrypt / decrypt sensitive data
public class SecureDataService(IEncryptionService encryption)
{
    public async Task<string> StoreSecretAsync(string plainText, CancellationToken ct)
        => await encryption.EncryptAsync(plainText, ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Security:Jwt:Issuer` | JWT issuer claim | — |
| `Security:Jwt:Audience` | JWT audience claim | — |
| `Security:Jwt:ExpiryMinutes` | Access token lifetime | `60` |
| `Security:Argon2:MemoryCost` | Argon2 memory cost (KB) | `65536` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
