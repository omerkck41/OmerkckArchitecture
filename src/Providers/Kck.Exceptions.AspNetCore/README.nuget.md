# Kck.Exceptions.AspNetCore

Centralized ASP.NET Core exception handling middleware that maps domain exceptions to RFC 7807 `ProblemDetails` responses.

## Installation

```bash
dotnet add package Kck.Exceptions.AspNetCore
```

## Quick Start

```csharp
using Kck.Exceptions;
using Kck.Exceptions.AspNetCore.Extensions;

// Program.cs
builder.Services.AddKckExceptionHandling();

var app = builder.Build();
app.UseKckExceptionHandling();   // first in the pipeline

// Throw domain exceptions anywhere: they become problem+json responses
app.MapGet("/orders/{id:guid}", (Guid id) =>
{
    throw new NotFoundException($"Order {id} not found.");   // 404
});
// BadRequestException 400, ForbiddenException 403, ConflictException 409, ValidationException 422,
// any other exception 500 with a generic detail.
```

## Configuration

No options. Status codes come from the exception type (`[HttpStatusCode]`) or its `explicitStatusCode`; other exceptions become a 500 with a generic detail, so internal messages are not exposed.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/exceptions.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
