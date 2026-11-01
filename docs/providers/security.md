# Security

A provider family for password hashing, JWT, TOTP 2FA, token blacklist and
secrets management.

## Packages

| Package | Purpose |
|---|---|
| `Kck.Security.Abstractions` | `IHashingService`, `ITokenService`, `IMfaProvider`, `ITokenBlacklistService`, `ISecretsManager` |
| `Kck.Security.Argon2` | Argon2id password hashing (`Konscious.Security.Cryptography.Argon2`) |
| `Kck.Security.Jwt` | RS256 JWT generation and validation (`JsonWebTokenHandler`) |
| `Kck.Security.Totp` | RFC 6238 TOTP (Google Authenticator compatible) |
| `Kck.Security.TokenBlacklist.Redis` | Redis-based logout / revoke |
| `Kck.Security.Secrets.UserSecrets` | `IConfiguration` (env, User Secrets, appsettings) |
| `Kck.Security.Secrets.AzureKeyVault` | Azure Key Vault |

## Argon2 (Passwords)

```csharp
services.AddKckArgon2(opt =>
{
    opt.MemorySize = 65536;        // KiB = 64 MiB (default; OWASP minimum 19 MiB)
    opt.Iterations = 3;            // default
    opt.DegreeOfParallelism = 1;   // default
});
```

Usage (note the argument order of `VerifyAsync`: the password first, then the stored hash):

```csharp
using Kck.Security.Abstractions.Hashing;

public class PasswordService(IHashingService hasher)
{
    public Task<string> HashAsync(string password, CancellationToken ct) => hasher.HashAsync(password, ct);

    public Task<bool> VerifyAsync(string password, string storedHash, CancellationToken ct) =>
        hasher.VerifyAsync(password, storedHash, ct);
}
```

**The package was kept** ([ADR-0001](../adr/0001-argon2-implementation.md)): there is no known
CVE and the migration risk is low.

## JWT

Tokens are signed with RS256 (an RSA private key); there is no shared-secret (HS256) mode.

```csharp
using Kck.Security.Jwt;

services.AddKckJwt(opt =>
{
    opt.Issuer = "myapp";
    opt.Audience = "myapp-api";
    opt.AccessTokenExpiration = TimeSpan.FromMinutes(15); // default
    opt.RefreshTokenTtlDays = 7;                          // default
    opt.KeySource = RsaKeySource.File;                    // or Configuration (default)
    opt.RsaKeyPath = builder.Configuration["Jwt:RsaKeyPath"];
});
```

- `RsaKeySource.Configuration`: `RsaKeyBase64` holds the PKCS#1 private key as Base64 (keep it in a secret store).
- `RsaKeySource.File`: `RsaKeyPath` points to a PEM file.
- `RsaKeySource.SecretsManager` is obsolete (KCK0300): it was never implemented, and options validation rejects it at
  startup. Read the key from your secret store yourself and set `RsaKeyBase64`.

Generation and validation:

```csharp
using Kck.Security.Abstractions.Token;

public class AuthService(ITokenService tokens)
{
    public async Task<string> SignInAsync(User user, CancellationToken ct)
    {
        TokenResult result = await tokens.CreateAccessTokenAsync(new TokenRequest
        {
            UserId = user.Id.ToString(),
            Email = user.Email,
            Roles = ["customer"]
        }, ct);
        return result.AccessToken; // ExpiresAt is in result.ExpiresAt
    }

    public Task<TokenValidationResult> ValidateAsync(string token, CancellationToken ct) =>
        tokens.ValidateTokenAsync(token, ct);
}
```

`CreateRefreshTokenAsync()` returns a random refresh token; storing and rotating it is up to you. Validation uses
`JsonWebTokenHandler` (`JwtSecurityTokenHandler` is deprecated,
[ADR-0003](../adr/0003-jwt-handler-migration.md)). `AddKckJwt` provides the token service only; ASP.NET Core
authentication is configured separately (`AddAuthentication().AddJwtBearer(...)`) with the matching public key.

`ValidateTokenAsync` returns `UserId`, `Email`, `Roles` (every role claim, in token order) and `Claims`: each claim
type of the token with its first value, so `CustomClaims` round-trip (`result.Claims["tenant"]`). For a multi-valued
type such as role, `Claims` holds the first value only; use `Roles`. Before 5.0.0, two or more roles came back as one
``List`1[System.Object]`` entry and `Claims` was always empty.

## TOTP (2FA)

TOTP needs a singleton `IMemoryCache` without a `SizeLimit` (its entries have no size) for replay protection:

```csharp
services.AddMemoryCache();
services.AddKckTotp(opt =>
{
    opt.CodeLength = 6;          // default
    opt.StepSeconds = 30;        // default
    opt.VerificationWindow = 1;  // steps accepted before and after the current one
    opt.MaxFailedAttempts = 5;   // opt-in (default 0 = no limit), per secret...
    opt.FailedAttemptWindow = TimeSpan.FromMinutes(5); // ...in a window from the first attempt (at most 1 day)
});
```

Usage:

```csharp
using Kck.Security.Abstractions.Mfa;

public class MfaService(IMfaProvider totp)
{
    // Show SetupResult.ProvisioningUri as a QR code; store SecretKey encrypted.
    public Task<MfaSetupResult> BeginSetupAsync(string email, CancellationToken ct) =>
        totp.GenerateSetupAsync(email, issuer: "MyApp", ct);

    public Task<bool> VerifyAsync(string secretKey, string code, CancellationToken ct) =>
        totp.ValidateCodeAsync(secretKey, code, ct);
}
```

Replay protection: a code accepted once is rejected for the rest of its validity window. The record is kept in
`IMemoryCache`, so it is per process: with several instances, a used code can be replayed on another instance within
its window.

> **Security:** with `VerificationWindow = 1`, three codes are valid at any moment, so a 6-digit code can be guessed
> by brute force. From 4.8.0, `MaxFailedAttempts` limits the codes checked per secret: each attempt is counted before
> its code is checked, and over the limit every code for that secret, a correct one too, is rejected until the window
> that began at the first attempt ends. A successful code resets the count, so an attacker guessing alongside the
> real user gets a new budget at each sign-in. The count is per process, like the replay record, and an attacker who
> keeps guessing also keeps the user out for that window. Rate-limit the verification endpoint per account as well, and lock the account (or require a
> fresh login) after repeated failures.

## Token Blacklist (Revoke / Logout)

```csharp
services.AddKckTokenBlacklistRedis(opt =>
{
    opt.ConnectionString = builder.Configuration["Redis:ConnectionString"]!;
    opt.KeyPrefix = "revoked:";   // default "kck:token:blacklist:"
});
```

The connection is opened asynchronously on first use, and a failed connect is retried on the next call. Leave
`ConnectionString` empty to share the `IConnectionMultiplexer` registered by Kck.Caching.Redis (available after the
host started). Sharing is possible but not recommended by default:

> **Warning — eviction makes revocation fail open.** A Redis instance shared with the cache must run with
> `maxmemory-policy noeviction`. Under `allkeys-lru` or `volatile-lru` (Azure Cache for Redis defaults to
> `volatile-lru`) the revocation keys, which carry a TTL, can be evicted under memory pressure, and a revoked token
> becomes valid again. A separate `Database` index does not prevent this: eviction works on the whole instance. Cache
> maintenance (`FLUSHDB`, prefix deletes such as `RemoveByPrefixAsync`) must never touch the blacklist's `KeyPrefix`.
> Prefer a separate Redis instance with `noeviction` for the blacklist.

```csharp
services.AddKckCachingRedis(redis => redis.Configuration = builder.Configuration["Redis:ConnectionString"]);
services.AddKckTokenBlacklistRedis(opt => opt.KeyPrefix = "revoked:"); // shares the Redis connection
```

Usage:

```csharp
using Kck.Security.Abstractions.Token;

public class LogoutService(ITokenBlacklistService blacklist)
{
    // Keep the entry until the token would have expired anyway.
    public Task RevokeAsync(string jti, DateTimeOffset expiresAt, CancellationToken ct) =>
        blacklist.RevokeAsync(jti, expiresAt - DateTimeOffset.UtcNow, ct);

    public Task<bool> IsRevokedAsync(string jti, CancellationToken ct) => blacklist.IsRevokedAsync(jti, ct);
}
```

When an `ITokenBlacklistService` is registered, `ITokenService.ValidateTokenAsync` rejects a token whose `jti` claim
is revoked (after the signature check). The ASP.NET Core JWT bearer handler does not consult it; check it in your own
`OnTokenValidated` event if bearer authentication must honor revocation.

## Secrets Management

### UserSecrets (`IConfiguration`)

```csharp
services.AddKckUserSecrets();

// .NET Secret Manager
// dotnet user-secrets set "Stripe:ApiKey" "sk_test_..."
```

Read-only: `SetSecretAsync` throws `NotSupportedException`. Combining environment variables
and User Secrets works well for local development. `GetSecretAsync<T>` deserializes the value as JSON, except
`T = string`, which returns the value as it is (a plain value such as an API key is not JSON); the Azure Key Vault
provider has the same contract.

### Azure Key Vault

```csharp
services.AddKckAzureKeyVault(opt =>
{
    opt.VaultUri = "https://my-vault.vault.azure.net/";
    opt.SecretPrefix = "prod";  // optional: GetSecretAsync("Stripe-ApiKey") reads "prod-Stripe-ApiKey"
});
```

Key Vault secret names allow only letters, digits and dashes, so use `-` instead of `:` in keys.

It uses `DefaultAzureCredential`, which automatically resolves the Azure CLI, Managed Identity or the
`AZURE_CLIENT_ID`/`AZURE_CLIENT_SECRET` environment variables.

## Compliance Notes

| Standard | Relevant Provider |
|---|---|
| OWASP ASVS | Argon2 (V2.4), JWT (V3.5), TOTP (V2.8) |
| PCI DSS | Secrets (KV), Argon2 (passwords) |
| GDPR | Secrets (KMS-backed via KV) |

To report a vulnerability in Kck itself, see the [security policy](https://github.com/omerkck41/OmerkckArchitecture/security/policy).
