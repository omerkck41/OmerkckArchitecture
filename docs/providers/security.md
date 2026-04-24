# Security

Parola hashleme, JWT, TOTP 2FA, token blacklist ve secrets yonetimi
icin provider ailesi.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Security.Abstractions` | `IHashingService`, `IJwtTokenService`, `IMfaProvider`, `ITokenBlacklistService`, `ISecretsManager` |
| `Kck.Security.Argon2` | Argon2id parola hashleme (`Konscious.Security.Cryptography.Argon2`) |
| `Kck.Security.Jwt` | JWT uret/dogrula (`JsonWebTokenHandler`) |
| `Kck.Security.Totp` | RFC 6238 TOTP (Google Authenticator uyumlu) |
| `Kck.Security.TokenBlacklist.Redis` | Redis tabanli logout / revoke |
| `Kck.Security.Secrets.UserSecrets` | `IConfiguration` (env, User Secrets, appsettings) |
| `Kck.Security.Secrets.AzureKeyVault` | Azure Key Vault |

## Argon2 (Parola)

```csharp
services.AddKckSecurityArgon2(opt =>
{
    opt.MemorySize = 65536;   // 64 MiB (OWASP min 19 MiB)
    opt.Iterations = 3;
    opt.DegreeOfParallelism = 4;
});
```

Kullanim:

```csharp
public class UserService(IHashingService hasher)
{
    public Task<string> HashAsync(string password) => hasher.HashAsync(password);
    public Task<bool> VerifyAsync(string hash, string password) =>
        hasher.VerifyAsync(hash, password);
}
```

**Paket korundu** ([ADR-0001](../adr/0001-argon2-implementation.md)) — bilinen
CVE yok, migration riski dusukluk nedeniyle.

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

Uretim:

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

Dogrulama `JsonWebTokenHandler` kullanir — `JwtSecurityTokenHandler` deprecated
([ADR-0003](../adr/0003-jwt-handler-migration.md)).

> **Guvenlik:** `SecretKey` en az 256-bit entropi (32+ byte random). RS256 /
> ES256 (asymmetric) senaryolari icin builder'i genislet. HS256 paylasimli
> secret yasak kural listesinde.

## TOTP (2FA)

```csharp
services.AddKckSecurityTotp(opt =>
{
    opt.Issuer = "MyApp";
    opt.DigitCount = 6;
    opt.TimeStep = 30;
});
```

Kullanim:

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

Replay koruma: son kullanilan kod cache'de saklanir
(`IReplayCache` — varsayilan InMemory).

## Token Blacklist (Revoke / Logout)

```csharp
services.AddKckSecurityTokenBlacklistRedis(opt =>
{
    opt.ConnectionString = builder.Configuration["Redis:ConnectionString"]!;
    opt.KeyPrefix = "revoked:";
});
```

Kullanim:

```csharp
public class LogoutService(ITokenBlacklistService blacklist)
{
    public Task RevokeAsync(string jti, DateTimeOffset expiresAt, CancellationToken ct)
        => blacklist.RevokeAsync(jti, expiresAt, ct);

    public Task<bool> IsRevokedAsync(string jti, CancellationToken ct)
        => blacklist.IsRevokedAsync(jti, ct);
}
```

JWT middleware `jti` claim'i uzerinden blacklist check eder.

## Secrets Yonetimi

### UserSecrets (`IConfiguration`)

```csharp
services.AddKckUserSecrets();

// .NET Secret Manager
// dotnet user-secrets set "Stripe:ApiKey" "sk_test_..."
```

Read-only — `SetSecretAsync` `NotSupportedException` firlatir. Env variable
ve User Secrets kombinasyonu local dev icin idealdir.

### Azure Key Vault

```csharp
services.AddKckAzureKeyVault(opt =>
{
    opt.VaultUri = "https://my-vault.vault.azure.net/";
    opt.SecretPrefix = "prod";  // opsiyonel: "prod-Stripe:ApiKey"
});
```

`DefaultAzureCredential` kullanir — Azure CLI, Managed Identity veya
`AZURE_CLIENT_ID`/`AZURE_CLIENT_SECRET` env variable'lari otomatik cozer.

## Compliance Notlari

| Standard | Ilgili Provider |
|---|---|
| OWASP ASVS | Argon2 (V2.4), JWT (V3.5), TOTP (V2.8) |
| PCI DSS | Secrets (KV), Argon2 (parola) |
| GDPR | Secrets (KV ile KMS-backed) |

Tum guvenlik kurallari icin [security.md](../../rules/security.md) global
rules bolumune bakilabilir (ozel dizinde degilse projeye kopyalanabilir).
