# Kck.Security.TokenBlacklist.Redis

Redis-backed JWT token blacklist for immediate token revocation on logout, password change, or security events.

## Installation

```bash
dotnet add package Kck.Security.TokenBlacklist.Redis
```

## Quick Start

```csharp
using Kck.Security.Abstractions.Token;

// Program.cs
builder.Services.AddKckTokenBlacklistRedis(options =>
{
    options.ConnectionString = "localhost:6379";
    options.KeyPrefix = "blacklist:";
});

// Revoke on logout: the token's jti claim until its expiry
app.MapPost("/logout", async (HttpContext http, ITokenBlacklistService blacklist, CancellationToken ct) =>
{
    var jti = http.User.FindFirst("jti")?.Value;
    var expires = http.User.FindFirst("exp")?.Value;
    if (jti is null || !long.TryParse(expires, out var exp))
        return Results.BadRequest();

    var remaining = DateTimeOffset.FromUnixTimeSeconds(exp) - DateTimeOffset.UtcNow;
    await blacklist.RevokeAsync(jti, remaining, ct);
    return Results.NoContent();
}).RequireAuthorization();
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ConnectionString` | Redis connection string | required |
| `KeyPrefix` | Prefix of the blacklist keys | `"kck:token:blacklist:"` |
| `Database` | Redis database index (`-1` = the connection's default) | `-1` |

A revoked `jti` is kept until the expiration passed to `RevokeAsync`; `ITokenService.ValidateTokenAsync` (Kck.Security.Jwt) rejects it.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
