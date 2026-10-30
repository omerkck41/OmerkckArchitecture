# Kck.Authorization.Abstractions

Permission-based authorization abstractions for the Kck framework — define IAuthorizationService, IPermissionChecker, and ICurrentUserProvider contracts without coupling to any auth provider.

## Installation

```bash
dotnet add package Kck.Authorization.Abstractions
```

## Quick Start

```csharp
using Kck.Authorization.Abstractions;
using Kck.Exceptions;

// Kck consumes these contracts (the Mediator AuthorizationBehavior reads ICurrentUserProvider) but does not
// implement them: register your own, typically over IHttpContextAccessor and your permission store.
builder.Services.AddScoped<IPermissionChecker, MyPermissionChecker>();

public class MyPermissionChecker(ICurrentUserProvider currentUser) : IPermissionChecker
{
    public Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default) =>
        Task.FromResult(currentUser.IsInRole("admin") || currentUser.Claims.ContainsKey($"perm:{permission}"));

    public async Task<bool> HasAllPermissionsAsync(IEnumerable<string> permissions, CancellationToken ct = default)
    {
        foreach (var permission in permissions)
            if (!await HasPermissionAsync(permission, ct)) return false;
        return true;
    }

    public async Task<bool> HasAnyPermissionAsync(IEnumerable<string> permissions, CancellationToken ct = default)
    {
        foreach (var permission in permissions)
            if (await HasPermissionAsync(permission, ct)) return true;
        return false;
    }
}

// Use in a handler
public class ReadOrdersHandler(IPermissionChecker permissions)
{
    public async Task HandleAsync(CancellationToken ct)
    {
        if (!await permissions.HasPermissionAsync("orders.read", ct))
            throw new ForbiddenException("Insufficient permissions.");
    }
}
```

## Configuration

This is a contracts-only package — no configuration required. Wire up concrete implementations in your provider package (e.g. `Kck.Security.Jwt`).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/security.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
