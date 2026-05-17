# Kck.Pipeline.MediatR

> **DEPRECATED** — Use `Kck.Pipeline.Mediator` instead. This package remains for backwards compatibility.

MediatR-backed CQRS pipeline with built-in validation, logging, and performance behaviors.

## Installation

```bash
dotnet add package Kck.Pipeline.MediatR
```

## Quick Start

```csharp
// Program.cs — prefer Kck.Pipeline.Mediator for new projects
builder.Services.AddKckPipeline(options =>
{
    options.RegisterServicesFromAssembly(typeof(Program).Assembly);
    options.EnableValidationBehavior = true;
    options.EnableLoggingBehavior = true;
});
```

## Migration to Kck.Pipeline.Mediator

Replace `AddKckPipeline()` with `AddKckMediator()` and update handler interfaces from MediatR types to `Mediator` types. See the [migration guide](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/pipeline.md).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/pipeline.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
