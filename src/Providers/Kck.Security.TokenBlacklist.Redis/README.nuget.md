# Kck.Security.TokenBlacklist.Redis

Redis-backed JWT token blacklist for immediate token revocation on logout, password change, or security events.

## Installation

```bash
dotnet add package Kck.Security.TokenBlacklist.Redis
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckTokenBlacklistRedis(options =>
{
    options.ConnectionString = "localhost:6379";
    options.KeyPrefix = "blacklist";
    options.Database = 1;
});

// Revoke on logout
public class AuthController(ITokenBlacklist blacklist)
{
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var token = HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        await blacklist.RevokeAsync(token, ct);
        return NoContent();
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ConnectionString` | Redis connection string | required |
| `KeyPrefix` | Prefix for blacklist keys in Redis | `"token:blacklist"` |
| `Database` | Redis database index | `0` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
