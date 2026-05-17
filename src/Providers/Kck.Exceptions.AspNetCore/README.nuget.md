# Kck.Exceptions.AspNetCore

Centralized ASP.NET Core exception handling middleware that maps domain exceptions to RFC 7807 `ProblemDetails` responses.

## Installation

```bash
dotnet add package Kck.Exceptions.AspNetCore
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckExceptionHandling();

var app = builder.Build();
app.UseKckExceptionHandling();

// Throw domain exceptions anywhere — they are mapped automatically
throw new NotFoundException($"Order {id} not found.");
throw new ValidationException("Name is required.");
throw new ForbiddenException("Access denied.");
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `IncludeStackTrace` | Include stack trace in development responses | `false` |
| `LogLevel` | Minimum log level for unhandled exceptions | `Error` |
| `ExposeExceptionMessage` | Expose raw exception message in production | `false` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/exceptions.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
