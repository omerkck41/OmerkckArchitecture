# Kck.Pipeline.MediatR

> **DEPRECATED — removed in v4.0.0.** Use `Kck.Pipeline.Mediator` (MIT) instead. This package remains only for
> backwards compatibility.
>
> **License warning:** this package depends on MediatR 14, which is licensed under the Reciprocal Public License 1.5
> or a paid Lucky Penny Software commercial license, and logs a warning at runtime when no license key is configured.
> Using `Kck.Pipeline.MediatR` means MediatR's terms apply to your application. See
> [docs/licensing.md](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/licensing.md).

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
