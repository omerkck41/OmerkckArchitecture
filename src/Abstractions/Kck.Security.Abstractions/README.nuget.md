# Kck.Security.Abstractions

Security abstractions — password hashing, encryption, JWT token management, token blacklist, secrets management, and MFA provider contracts.

## Installation

```bash
dotnet add package Kck.Security.Abstractions
```

## Quick Start

```csharp
using Kck.Security.Abstractions.Hashing;
using Kck.Security.Abstractions.Token;

// Program.cs — the providers (Kck.Security.Argon2, Kck.Security.Jwt)
builder.Services.AddKckArgon2();
builder.Services.AddKckJwt(opt =>
{
    opt.Issuer = "https://auth.example.com";
    opt.Audience = "orders-api";
    opt.RsaKeyBase64 = builder.Configuration["Jwt:RsaKeyBase64"]; // PKCS#1 private key, from a secret store
});

public class AuthService(IHashingService hasher, ITokenService tokens)
{
    public Task<string> HashAsync(string password, CancellationToken ct) => hasher.HashAsync(password, ct);

    // Password first, then the stored hash
    public Task<bool> VerifyAsync(string password, string storedHash, CancellationToken ct) =>
        hasher.VerifyAsync(password, storedHash, ct);

    public Task<TokenResult> IssueTokenAsync(string userId, string email, CancellationToken ct) =>
        tokens.CreateAccessTokenAsync(new TokenRequest { UserId = userId, Email = email }, ct);
}

// ISecretsManager and ITokenBlacklistService are implemented by
// Kck.Security.Secrets.* and Kck.Security.TokenBlacklist.Redis.
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
