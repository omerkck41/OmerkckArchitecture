# Security

A provider family for password hashing, JWT, TOTP 2FA, token blacklist and
secrets management.

## Packages

| Package | Purpose |
|---|---|
| `Kck.Security.Abstractions` | `IHashingService`, `IJwtTokenService`, `IMfaProvider`, `ITokenBlacklistService`, `ISecretsManager` |
| `Kck.Security.Argon2` | Argon2id password hashing (`Konscious.Security.Cryptography.Argon2`) |
| `Kck.Security.Jwt` | JWT generation and validation (`JsonWebTokenHandler`) |
| `Kck.Security.Totp` | RFC 6238 TOTP (Google Authenticator compatible) |
| `Kck.Security.TokenBlacklist.Redis` | Redis-based logout / revoke |
| `Kck.Security.Secrets.UserSecrets` | `IConfiguration` (env, User Secrets, appsettings) |
| `Kck.Security.Secrets.AzureKeyVault` | Azure Key Vault |

## Argon2 (Passwords)

```csharp
services.AddKckSecurityArgon2(opt =>
{
    opt.MemorySize = 65536;   // 64 MiB (OWASP minimum 19 MiB)
    opt.Iterations = 3;
    opt.DegreeOfParallelism = 4;
});
```

Usage:

```csharp
public class UserService(IHashingService hasher)
{
    public Task<string> HashAsync(string password) => hasher.HashAsync(password);
    public Task<bool> VerifyAsync(string hash, string password) =>
        hasher.VerifyAsync(hash, password);
}
```

**The package was kept** ([ADR-0001](../adr/0001-argon2-implementation.md)): there is no known
CVE and the migration risk is low.

## JWT

```csharp
services.AddKckSecurityJwt(opt =>
{
    opt.Issuer = "myapp";
    opt.Audience = "myapp-api";
    opt.SecretKey = builder.Configuration["Jwt:SecretKey"]!;
    opt.AccessTokenLifetime = TimeSpan.FromMinutes(15);
    opt.RefreshTokenLifetime = TimeSpan.FromDays(7);
});
```

Generation:

```csharp
public class AuthService(IJwtTokenService jwt)
{
    public string GenerateAccessToken(User user) =>
        jwt.GenerateToken(new Claim[]
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email)
        });
}
```

Validation uses `JsonWebTokenHandler`. `JwtSecurityTokenHandler` is deprecated
([ADR-0003](../adr/0003-jwt-handler-migration.md)).

> **Security:** `SecretKey` needs at least 256 bits of entropy (32+ random bytes). For RS256 /
> ES256 (asymmetric) scenarios, extend the builder. HS256 with a shared
> secret is on the prohibited rules list.

## TOTP (2FA)

```csharp
services.AddKckSecurityTotp(opt =>
{
    opt.Issuer = "MyApp";
    opt.DigitCount = 6;
    opt.TimeStep = 30;
});
```

Usage:

```csharp
public class MfaService(IMfaProvider totp)
{
    public Task<string> GenerateSecretAsync() => totp.GenerateSecretAsync();

    public async Task<bool> VerifyAsync(string secret, string code)
    {
        return await totp.ValidateAsync(secret, code);
    }
}
```

Replay protection: the last used code is stored in a cache
(`IReplayCache`; the default is InMemory).

## Token Blacklist (Revoke / Logout)

```csharp
services.AddKckSecurityTokenBlacklistRedis(opt =>
{
    opt.ConnectionString = builder.Configuration["Redis:ConnectionString"]!;
    opt.KeyPrefix = "revoked:";
});
```

Usage:

```csharp
public class LogoutService(ITokenBlacklistService blacklist)
{
    public Task RevokeAsync(string jti, DateTimeOffset expiresAt, CancellationToken ct)
        => blacklist.RevokeAsync(jti, expiresAt, ct);

    public Task<bool> IsRevokedAsync(string jti, CancellationToken ct)
        => blacklist.IsRevokedAsync(jti, ct);
}
```

The JWT middleware checks the blacklist using the `jti` claim.

## Secrets Management

### UserSecrets (`IConfiguration`)

```csharp
services.AddKckUserSecrets();

// .NET Secret Manager
// dotnet user-secrets set "Stripe:ApiKey" "sk_test_..."
```

Read-only: `SetSecretAsync` throws `NotSupportedException`. Combining environment variables
and User Secrets works well for local development.

### Azure Key Vault

```csharp
services.AddKckAzureKeyVault(opt =>
{
    opt.VaultUri = "https://my-vault.vault.azure.net/";
    opt.SecretPrefix = "prod";  // optional: "prod-Stripe:ApiKey"
});
```

It uses `DefaultAzureCredential`, which automatically resolves the Azure CLI, Managed Identity or the
`AZURE_CLIENT_ID`/`AZURE_CLIENT_SECRET` environment variables.

## Compliance Notes

| Standard | Relevant Provider |
|---|---|
| OWASP ASVS | Argon2 (V2.4), JWT (V3.5), TOTP (V2.8) |
| PCI DSS | Secrets (KV), Argon2 (passwords) |
| GDPR | Secrets (KMS-backed via KV) |

For all security rules, see the global `rules/security.md` rules section
(if it is not in a dedicated directory, you can copy it into your project).
