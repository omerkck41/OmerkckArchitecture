# Kck.Authorization.Abstractions

Permission-based authorization abstractions for the Kck framework — define IAuthorizationService, IPermissionChecker, and ICurrentUserProvider contracts without coupling to any auth provider.

## Installation

```bash
dotnet add package Kck.Authorization.Abstractions
```

## Quick Start

```csharp
// Implement the contracts in your auth provider
public class MyPermissionChecker : IPermissionChecker
{
    public async Task<bool> HasPermissionAsync(string userId, string permission,
        CancellationToken ct = default)
    {
        // query your permission store
        return await _store.ExistsAsync(userId, permission, ct);
    }
}

// Register in Program.cs
builder.Services.AddScoped<IPermissionChecker, MyPermissionChecker>();
builder.Services.AddScoped<ICurrentUserProvider, HttpContextCurrentUserProvider>();

// Use in a handler or controller
public class MyHandler(IPermissionChecker permissions, ICurrentUserProvider currentUser)
{
    public async Task HandleAsync(CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        if (!await permissions.HasPermissionAsync(userId, "orders.read", ct))
            throw new UnauthorizedException("Insufficient permissions.");
    }
}
```

## Configuration

This is a contracts-only package — no configuration required. Wire up concrete implementations in your provider package (e.g. `Kck.Security.Jwt`).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
