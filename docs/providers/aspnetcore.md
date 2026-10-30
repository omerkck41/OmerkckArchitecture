# Web (ASP.NET Core)

ASP.NET Core building blocks: security headers, per-IP rate limiting, CORS, and opt-in helpers (input sanitizer, cookie
and session managers, API responses).

## Packages

| Package | Purpose |
|---|---|
| `Kck.AspNetCore` | `AddKckAspNetCore(...)` builder, `UseKckAspNetCore()`, `ApiResponse<T>`, `KckApiControllerBase` |
| `Kck.Authorization.Abstractions` | `ICurrentUserProvider` (you implement it) |

## Setup

```csharp
services.AddKckAspNetCore(aspnet => aspnet
    .UseSecurityHeaders()
    .UseRateLimiting(rate =>
    {
        rate.PermitLimit = 100;      // requests per client IP...
        rate.WindowInSeconds = 60;   // ...per fixed window; then 429
    })
    .UseCorsPolicy(["https://app.myapp.com", "https://admin.myapp.com"]));

// ...

app.UseKckAspNetCore();
```

Every feature is opt-in: `UseKckAspNetCore()` adds only what the builder enabled, in this order:

1. Security headers: `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy`,
   `Permissions-Policy`, `Content-Security-Policy: default-src 'self'`, and HSTS on HTTPS requests in Production.
2. Rate limiter: a global fixed-window limiter partitioned by the connection's remote IP. Behind a reverse proxy, call
   `app.UseForwardedHeaders()` first, otherwise every request shares the proxy's IP. List your proxies in
   `ForwardedHeadersOptions.KnownProxies` / `KnownNetworks` (only loopback is trusted by default); never trust every
   source, or clients can spoof `X-Forwarded-For` and get a fresh rate-limit partition per request.
3. CORS: the named policy (`KckCorsPolicy` by default) allows the listed origins with any method and header.
   `UseCorsPolicy()` without origins allows **any** origin and logs a warning in Production.

Call `UseKckAspNetCore()` after your exception handler and before `UseAuthentication()`, `UseAuthorization()` and the
endpoints. It does not add authentication, health checks or HTTP logging.

### Optional services

```csharp
services.AddKckAspNetCore(aspnet => aspnet
    .AddInputSanitizer()     // IInputSanitizer
    .AddCookieManager()      // ICookieManager
    .AddSessionManager());   // ISessionManager
```

## Current User

`ICurrentUserProvider` is an abstraction that Kck consumes (the Mediator `AuthorizationBehavior` reads roles from it)
but does not implement: register your own, typically over `IHttpContextAccessor`.

```csharp
using System.Security.Claims;
using Kck.Authorization.Abstractions;

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

services.AddHttpContextAccessor();
services.AddScoped<ICurrentUserProvider, HttpCurrentUserProvider>();
```

## WebApi Bundle

For an opinionated setup in one call, use [`Kck.Bundle.WebApi`](https://github.com/omerkck41/OmerkckArchitecture/tree/main/src/Bundles/Kck.Bundle.WebApi):

```csharp
builder.Services.AddKckWebApiDefaults(builder.Configuration);
// ...
app.UseKckWebApiDefaults();
```

`AddKckWebApiDefaults` registers Serilog, the in-memory cache, Argon2 hashing, the in-memory event bus, exception
handling, health checks, and this package with rate limiting (security headers and CORS from the `Kck:AspNetCore`
configuration section). The JWT token service is added when `Kck:Security:Jwt:Issuer` is set; it does not configure
ASP.NET Core authentication (add `AddAuthentication().AddJwtBearer()` yourself). `UseKckWebApiDefaults()` runs the
exception handler, then `UseKckAspNetCore()`.
