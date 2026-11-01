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
| `ConnectionString` | Redis connection string | required unless an `IConnectionMultiplexer` is registered (then it is shared) |
| `KeyPrefix` | Prefix of the blacklist keys | `"kck:token:blacklist:"` |
| `Database` | Redis database index (`-1` = the connection's default) | `-1` |

A revoked `jti` is kept until the expiration passed to `RevokeAsync`; `ITokenService.ValidateTokenAsync` (Kck.Security.Jwt) rejects it.

## Connection

With `ConnectionString` set, the blacklist opens its own connection asynchronously on first use; a failed connect is
retried on the next call. With Kck.Caching.Redis (`AddKckCachingRedis`), you can leave `ConnectionString` empty to
share its connection instead; that connection is available after the host started. Sharing is possible but not
recommended by default:

> **Warning — eviction makes revocation fail open.** A Redis instance shared with the cache must run with
> `maxmemory-policy noeviction`. Under `allkeys-lru` or `volatile-lru` (Azure Cache for Redis defaults to
> `volatile-lru`) the revocation keys, which carry a TTL, can be evicted under memory pressure, and a revoked token
> becomes valid again. A separate `Database` index does not prevent this. Cache maintenance (`FLUSHDB`, prefix deletes)
> must never touch the blacklist's `KeyPrefix`. Prefer a separate Redis instance with `noeviction` for the blacklist.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
