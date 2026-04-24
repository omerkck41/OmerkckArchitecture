# Web (ASP.NET Core)

Framework-level ASP.NET Core entegrasyonu — startup extension'lari,
current user provider, HTTP context accessor helper.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.AspNetCore` | `AddKckWebApi()`, `UseKckWebApi()`, current user accessor |

## Setup

```csharp
services.AddKckAspNetCore(opt =>
{
    opt.EnableCors = true;
    opt.CorsOrigins = ["https://app.myapp.com"];
    opt.EnableHttpLogging = false;
});

// ...

app.UseKckAspNetCore();
```

## Current User

```csharp
public class AuditService(ICurrentUserProvider user)
{
    public string? GetUserId() => user.UserId;
    public string? GetTenantId() => user.TenantId;
    public bool IsAuthenticated() => user.IsAuthenticated;
}
```

`ICurrentUserProvider` `HttpContextAccessor` uzerinden claim'leri okur — DI
scope per request.

## CORS

`EnableCors = true` iken:

```csharp
opt.CorsOrigins = ["https://app.myapp.com", "https://admin.myapp.com"];
opt.CorsAllowCredentials = true;
opt.CorsAllowedHeaders = ["Content-Type", "Authorization"];
```

`UseKckAspNetCore()` CORS middleware'i otomatik ekler (sirasi: Exception
handler → CORS → AuthN → AuthZ → Endpoints).

## HTTP Logging

Production'da default **kapali** (performans + PII). Debug senaryolar icin:

```csharp
opt.EnableHttpLogging = true;
opt.HttpLoggingFields = HttpLoggingFields.Request | HttpLoggingFields.Response;
opt.HttpLoggingMediaTypes = ["application/json"];
```

## Middleware Sirasi

`UseKckAspNetCore()` asagidaki sirayi garanti eder:

1. Exception handler (from `Kck.Exceptions.AspNetCore`)
2. HTTPS redirect
3. Static files (opsiyonel)
4. Routing
5. CORS
6. Authentication
7. Authorization
8. Health checks (`/health/live`, `/health/ready`)
9. Endpoints

## WebApi Bundle

Daha yuksek-seviye opinionated setup icin [`Kck.Bundle.WebApi`](../../src/Bundles/Kck.Bundle.WebApi):

```csharp
services.AddKckWebApi(opt => { /* ... */ });
```

Bu bundle:

- MediatR pipeline
- Exception handler
- Serilog (JSON)
- OpenTelemetry (ASP.NET + HttpClient + EF)
- JWT authentication

tek cagriyla opinionated defaults ile kaydeder.
