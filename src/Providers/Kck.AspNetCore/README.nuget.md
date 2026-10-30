# Kck.AspNetCore

ASP.NET Core enhancement provider for Kck applications — adds rate limiting, security headers middleware, CORS policy, and a standardized `ApiResponse<T>` controller base.

## Installation

```bash
dotnet add package Kck.AspNetCore
```

## Quick Start

```csharp
// Program.cs - every feature is opt-in
builder.Services.AddKckAspNetCore(aspnet => aspnet
    .UseSecurityHeaders()
    .UseRateLimiting(rate => rate.PermitLimit = 100)   // per client IP, per 60 s window
    .UseCorsPolicy(["https://myapp.com"]));

var app = builder.Build();
app.UseKckAspNetCore();   // security headers, then rate limiter, then CORS
```

## Configuration

The builder enables each feature; nothing is on by default.

| Builder method | Effect |
|---|---|
| `UseSecurityHeaders()` | `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`, CSP; HSTS on HTTPS in Production |
| `UseRateLimiting(o => ...)` | Fixed window per client IP: `PermitLimit` (100) requests per `WindowInSeconds` (60); then 429 |
| `UseCorsPolicy(origins)` | CORS policy for the origins; without origins, any origin (warning in Production) |
| `AddInputSanitizer()` / `AddCookieManager()` / `AddSessionManager()` | Registers the helper service |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/aspnetcore.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
