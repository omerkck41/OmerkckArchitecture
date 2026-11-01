# Exceptions

A domain exception hierarchy plus an ASP.NET Core global exception handler.
They are split so that the abstraction does not leak into the provider layer
([ADR-0008](../adr/0008-exceptions-abstractions-split.md)).

## Packages

| Package | Purpose |
|---|---|
| `Kck.Exceptions.Abstractions` | Domain exception hierarchy |
| `Kck.Exceptions.AspNetCore` | Exception middleware with ProblemDetails responses |

## Domain Exception Hierarchy

| Type | HTTP Status | Usage |
|---|---|---|
| `BadRequestException` | 400 | Invalid input |
| `CustomArgumentException` | 400 | Invalid argument |
| `CustomInvalidOperationException` | 400 | Operation not valid in the current state |
| `UnauthorizedException` | 401 | Not authenticated |
| `SecurityTokenException` | 401 | Invalid auth token |
| `ForbiddenException` | 403 | Not authorized |
| `NotFoundException` | 404 | Record not found |
| `RequestTimeoutException` | 408 | Timeout |
| `ConflictException` | 409 | Duplicate / conflict |
| `ValidationException` | 422 | Per-field validation errors (see below) |
| `TooManyRequestsException` | 429 | Rate limited |

All of them derive from `CustomException` (namespace `Kck.Exceptions`). The status code comes from the
`[HttpStatusCode]` attribute on the type, or from the `explicitStatusCode` constructor argument; a `CustomException`
with neither is a 500. Extend it for your own domain exceptions:

```csharp
using Kck.Exceptions;
using Kck.Exceptions.Attributes;

[HttpStatusCode(StatusCodes.Status422UnprocessableEntity)]
public sealed class InsufficientBalanceException(Guid accountId)
    : CustomException($"Account {accountId} has insufficient balance");
```

## ASP.NET Core Middleware

```csharp
using Kck.Exceptions.AspNetCore.Extensions;

builder.Services.AddKckExceptionHandling();
// ...
app.UseKckExceptionHandling(); // first in the pipeline, so it sees every exception
```

The middleware writes `application/problem+json` (RFC 7807 shape). For a `CustomException`, `title` is the exception
type name and `detail` its message:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "NotFoundException",
  "status": 404,
  "detail": "User with id 42 was not found",
  "instance": "/users/42",
  "traceId": "0HN..."
}
```

Any other exception becomes a 500 with the fixed detail `An unexpected error occurred.`, so internal messages do not
leak. An `OperationCanceledException` is rethrown only when the request itself was aborted; another one (an
`HttpClient` timeout) becomes a 500. Another error in an aborted request is logged, but no response is written: the
client is gone. If the response has already started, the exception is logged and rethrown. `UseKckExceptionHandling()` also enables `UseStatusCodePages()`.

## ValidationException

`ValidationException` carries a list of `ValidationExceptionModel { Property, Errors }` and is answered with 422,
`"title": "Validation Error"` and an `errors` list per field. The Mediator `ValidationBehavior`
([pipeline](pipeline.md)) throws it when FluentValidation validators fail; you can also throw it yourself.

## Custom Responses

The handlers are fixed (`ValidationExceptionHandler`, then `GlobalExceptionHandler`); there is no handler
registration point. Map your own failures to a `CustomException` subclass with the status code you want. For a fully
custom response, put your own middleware or ASP.NET Core `IExceptionHandler` (with `app.UseExceptionHandler()`) before
`UseKckExceptionHandling()`.

## Logging

Every caught exception is logged once, with the request's TraceId (`TraceIdentifier`): at `Warning` when it maps to a
4xx status (a `CustomException` or `ValidationException`), at `Error` for a 5xx (any other exception, or a
`CustomException` without a status code). Before 4.7.0 the middleware also logged every exception at `Error`. If the
response has already started, the middleware logs the exception at `Error` and rethrows it (the host logs it too).
