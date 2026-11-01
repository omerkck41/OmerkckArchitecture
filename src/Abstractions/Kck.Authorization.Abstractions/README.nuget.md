# Kck.Authorization.Abstractions

Authorization abstractions for the Kck framework — define the ICurrentUserProvider contract without coupling to any auth provider.

## Installation

```bash
dotnet add package Kck.Authorization.Abstractions
```

## Quick Start

```csharp
using System.Security.Claims;
using Kck.Authorization.Abstractions;
using Kck.Exceptions;

// Kck consumes ICurrentUserProvider (the Mediator AuthorizationBehavior reads it) but does not implement it:
// register your own, typically over IHttpContextAccessor.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserProvider, HttpCurrentUserProvider>();

public sealed class HttpCurrentUserProvider(IHttpContextAccessor accessor) : ICurrentUserProvider
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public string? Name => User?.Identity?.Name;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];
    public IReadOnlyDictionary<string, string> Claims =>
        User?.Claims.DistinctBy(c => c.Type).ToDictionary(c => c.Type, c => c.Value) ?? [];
    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}

// Use in a handler: check the current user's roles or permission claims
public class ReadOrdersHandler(ICurrentUserProvider currentUser)
{
    public void EnsureCanRead()
    {
        if (!currentUser.IsInRole("admin") && !currentUser.Claims.ContainsKey("perm:orders.read"))
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
