# Web (ASP.NET Core)

Framework-level ASP.NET Core integration: startup extensions,
a current user provider, and an HTTP context accessor helper.

## Packages

| Package | Purpose |
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

`ICurrentUserProvider` reads claims through `HttpContextAccessor`. It is
registered per request in the DI scope.

## CORS

When `EnableCors = true`:

```csharp
opt.CorsOrigins = ["https://app.myapp.com", "https://admin.myapp.com"];
opt.CorsAllowCredentials = true;
opt.CorsAllowedHeaders = ["Content-Type", "Authorization"];
```

`UseKckAspNetCore()` adds the CORS middleware automatically (order: Exception
handler → CORS → AuthN → AuthZ → Endpoints).

## HTTP Logging

In production it is **off** by default (performance and PII). For debugging
scenarios:

```csharp
opt.EnableHttpLogging = true;
opt.HttpLoggingFields = HttpLoggingFields.Request | HttpLoggingFields.Response;
opt.HttpLoggingMediaTypes = ["application/json"];
```

## Middleware Order

`UseKckAspNetCore()` guarantees the following order:

1. Exception handler (from `Kck.Exceptions.AspNetCore`)
2. HTTPS redirect
3. Static files (optional)
4. Routing
5. CORS
6. Authentication
7. Authorization
8. Health checks (`/health/live`, `/health/ready`)
9. Endpoints

## WebApi Bundle

For a higher-level, opinionated setup, use [`Kck.Bundle.WebApi`](../../src/Bundles/Kck.Bundle.WebApi):

```csharp
services.AddKckWebApi(opt => { /* ... */ });
```

This bundle:

- Mediator pipeline
- Exception handler
- Serilog (JSON)
- OpenTelemetry (ASP.NET + HttpClient + EF)
- JWT authentication

registers all of the above with opinionated defaults in a single call.
