# Exceptions

A domain exception hierarchy plus an ASP.NET Core global exception handler.
They are split so that the abstraction does not leak into the provider layer
([ADR-0008](../adr/0008-exceptions-abstractions-split.md)).

## Packages

| Package | Purpose |
|---|---|
| `Kck.Exceptions.Abstractions` | Domain exception hierarchy |
| `Kck.Exceptions.AspNetCore` | Middleware + `IExceptionHandler` |

## Domain Exception Hierarchy

| Type | HTTP Status | Usage |
|---|---|---|
| `BadRequestException` | 400 | Invalid input |
| `CustomArgumentException` | 400 | Validation error |
| `NotFoundException` | 404 | Record not found |
| `ConflictException` | 409 | Duplicate / conflict |
| `ForbiddenException` | 403 | Not authorized |
| `RequestTimeoutException` | 408 | Timeout |
| `SecurityTokenException` | 401 | Invalid auth token |
| `CustomInvalidOperationException` | 500 | Unexpected state |
| `FilterSecurityException` | 403 | Query filter violation |

All of them derive from the `CustomException` base. You can extend it for your
own domain exceptions:

```csharp
public sealed class InsufficientBalanceException(Guid accountId)
    : CustomException($"Account {accountId} has insufficient balance");
```

## ASP.NET Core Middleware

```csharp
services.AddKckExceptionHandling();
// ...
app.UseKckExceptionHandler();
```

The middleware serializes exceptions in RFC 7807 ProblemDetails format:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Not Found",
  "status": 404,
  "detail": "User with id 42 was not found",
  "traceId": "00-..."
}
```

## ValidationException Handler

If you use FluentValidation, validation errors are thrown as `ValidationException`
and caught by a separate `IExceptionHandler`. It returns 400 plus a per-field error
list.

## Custom Handler

```csharp
public class MyCustomHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception ex, CancellationToken ct)
    {
        if (ex is MyDomainException mde)
        {
            ctx.Response.StatusCode = 422;
            await ctx.Response.WriteAsJsonAsync(new { mde.Message }, ct);
            return true;
        }
        return false;
    }
}

services.AddSingleton<IExceptionHandler, MyCustomHandler>();
```

Handlers are tried in order. The first one that returns `true` wins and the rest are skipped.

## Logging

The middleware logs the error with `ILogger`:
- 4xx: `Warning` (client error)
- 5xx: `Error` (server error)
- It includes the TraceId (for distributed tracing)
