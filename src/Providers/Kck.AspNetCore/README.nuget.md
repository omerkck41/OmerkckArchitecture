# Kck.AspNetCore

ASP.NET Core enhancement provider for Kck applications — adds rate limiting, security headers middleware, CORS policy, and a standardized `ApiResponse<T>` controller base.

## Installation

```bash
dotnet add package Kck.AspNetCore
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckAspNetCore(options =>
{
    options.EnableRateLimiting = true;
    options.EnableSecurityHeaders = true;
    options.CorsOrigins = ["https://myapp.com"];
});

var app = builder.Build();
app.UseKckSecurityHeaders();
app.UseKckCors();
app.UseKckRateLimiting();
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `EnableRateLimiting` | Activates sliding-window rate limiting | `true` |
| `EnableSecurityHeaders` | Adds HSTS, CSP, X-Frame-Options headers | `true` |
| `CorsOrigins` | Allowed CORS origins | `[]` |
| `RateLimitWindowSeconds` | Rate limit sliding window size in seconds | `60` |
| `RateLimitMaxRequests` | Maximum requests per window | `100` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/aspnetcore.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
