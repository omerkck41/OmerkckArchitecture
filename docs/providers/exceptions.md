# Exceptions

Domain exception hiyerarsisi + ASP.NET Core global exception handler.
Provider layer'a sizmayan abstraction ile ayrildi
([ADR-0008](../adr/0008-exceptions-abstractions-split.md)).

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Exceptions.Abstractions` | Domain exception hiyerarsisi |
| `Kck.Exceptions.AspNetCore` | Middleware + `IExceptionHandler` |

## Domain Exception Hiyerarsisi

| Tip | HTTP Status | Kullanim |
|---|---|---|
| `BadRequestException` | 400 | Gecersiz input |
| `CustomArgumentException` | 400 | Dogrulama hatasi |
| `NotFoundException` | 404 | Kayit bulunamadi |
| `ConflictException` | 409 | Duplicate / conflict |
| `ForbiddenException` | 403 | Yetki yok |
| `RequestTimeoutException` | 408 | Timeout |
| `SecurityTokenException` | 401 | Auth token invalid |
| `CustomInvalidOperationException` | 500 | Beklenmedik durum |
| `FilterSecurityException` | 403 | Query filter ihlal |

Tum `CustomException` base'ten turetilir — kendi domain exception'larin icin
genisletebilirsin:

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

Middleware exceptionlari RFC 7807 ProblemDetails formatinda serialize eder:

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

FluentValidation kullaniyorsan, validation hatalari `ValidationException` olarak
firlar ve ayri `IExceptionHandler` ile yakalanir — 400 + field bazli hata
listesi doner.

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

Handler'lar sirayla denenir — ilk `true` donen kazanir, digerleri atlanir.

## Loglama

Middleware hatayi `ILogger` ile loglar:
- 4xx: `Warning` (client hatasi)
- 5xx: `Error` (server hatasi)
- TraceId ekliyor (distributed tracing icin)
