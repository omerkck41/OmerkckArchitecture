# Kck Modular Architecture Framework

[![Build](https://img.shields.io/github/actions/workflow/status/omerkck41/OmerkckArchitecture/build-test.yml?branch=main)](https://github.com/omerkck41/OmerkckArchitecture/actions/workflows/build-test.yml)
[![NuGet](https://img.shields.io/nuget/v/Kck.Core.Abstractions.svg)](https://www.nuget.org/packages/Kck.Core.Abstractions/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/omerkck41/OmerkckArchitecture/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)

Modular, extensible .NET framework organized around an **Abstractions → Providers → Bundles** pattern. Abstractions declare contracts; providers implement them against concrete technologies; bundles compose opinionated defaults for specific hosting models (WebApi, WorkerService, etc.).

## Quick Start

### Fastest start: the template

```bash
dotnet new install Kck.Templates
dotnet new kck-webapi -n Shop && cd Shop
dotnet run --project src/Shop.Api   # API reference at /scalar, health at /health/ready
dotnet test
```

A solution on SQLite with nothing to set up: EF Core persistence with soft delete, audit fields and domain events
dispatched after commit, an order sample, JWT bearer authentication and integration tests
([what's inside](https://github.com/omerkck41/OmerkckArchitecture/blob/main/src/Kck.Templates/README.md)).

### Web API (ASP.NET Core, full Bundle)

```bash
dotnet add package Kck.Bundle.WebApi
dotnet add package Kck.Messaging.MailKit
```

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddKckWebApiDefaults(builder.Configuration)   // Serilog, InMemory cache, JWT, Argon2, event bus, exceptions, OTel
    .AddKckMessagingMailKit(opt =>
    {
        opt.Host = builder.Configuration["Mail:Host"]!;
        opt.Port = 587;
        opt.UseSsl = true;
    });

var app = builder.Build();
app.UseKckWebApiDefaults();                        // exception handling + Kck.AspNetCore middleware
app.MapGet("/", () => Results.Ok("Hello from Kck!"));
app.Run();
```

> Full sample: [`samples/Kck.Sample.Orders`](https://github.com/omerkck41/OmerkckArchitecture/tree/main/samples/Kck.Sample.Orders)

### Minimal API (Slim, only what you need)

```bash
dotnet add package Kck.Exceptions.AspNetCore
dotnet add package Kck.Caching.InMemory
dotnet add package Kck.Pipeline.Mediator
dotnet add package Mediator.SourceGenerator    # generates AddMediator() and the dispatcher
```

```csharp
using Kck.Exceptions.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddKckExceptionHandling()
    .AddKckCachingInMemory()
    .AddMediator()                          // Mediator source-gen dispatcher
    .AddKckMediatorPipeline(p => p
        .UseValidationBehavior()
        .UseLoggingBehavior());

var app = builder.Build();
app.UseKckExceptionHandling();
app.MapPost("/users", async (CreateUserCommand cmd, IMediator mediator) =>
    Results.Ok(await mediator.Send(cmd)));
app.Run();
```

> Full sample: [`samples/Kck.Sample.MinimalApi`](https://github.com/omerkck41/OmerkckArchitecture/tree/main/samples/Kck.Sample.MinimalApi)

### Worker Service (background jobs, no HTTP)

```bash
dotnet add package Kck.Bundle.WorkerService
```

```csharp
var builder = Host.CreateApplicationBuilder(args);

// Serilog + OpenTelemetry + InMemory event bus + Quartz.NET scheduler
builder.Services.AddKckWorkerServiceDefaults(builder.Configuration);
builder.Services.AddKckJob<EmailDigestJob>();

var host = builder.Build();
await host.Services.GetRequiredService<IJobScheduler>()
    .AddOrUpdateRecurringAsync<EmailDigestJob>("email-digest", "0 0 9 * * ?"); // every day at 09:00
await host.RunAsync();
```

> Full sample: [`samples/Kck.Sample.WorkerService`](https://github.com/omerkck41/OmerkckArchitecture/tree/main/samples/Kck.Sample.WorkerService)

### Target Frameworks

| Packages | TFM |
|---|---|
| All packages | `net10.0` |

The library targets `net10.0` and later only ([ADR-0019](docs/adr/0019-drop-net8-target.md)).

## Why Kck?

Kck aims to be **lighter** than ABP, **broader** than FastEndpoints, and **compatible** with Aspire:

| Topic | Kck | ABP Framework | FastEndpoints | Microsoft .NET Aspire |
|---|---|---|---|---|
| **Licence** | MIT | LGPL (Pro: paid) | MIT | MIT |
| **Approach** | Modular, opt-in | All-in-one, opinionated | Endpoint-only | Orchestration |
| **DI conventions** | `Microsoft.Extensions.*` native | ABP DI extensions | `Microsoft.Extensions.*` | `Microsoft.Extensions.*` |
| **Source generation / proxies** | None (plain C#) | Many (UoW interceptor, dynamic proxy) | None | None |
| **Target frameworks** | net10.0 | net8 + net9 | net8 + net9 + net10 | net8 + net9 + net10 |
| **Native AOT** | Partly (abstractions + Mediator, in-memory cache / event bus / feature flags, Argon2, TOTP, JWT, Localization(.Json), Polly, Http.Resilience; native AOT smoke in CI) | No | Yes | Yes |
| **Providers** | 30+ official | 15+ official (Pro) | N/A | N/A (orchestration) |
| **Batteries included** | Through opt-in modules | All-in-one | HTTP only | None |
| **Source Link + symbols** | Yes | Yes | Yes | Yes |
| **Learning curve** | Low to medium | High | Low | Medium |

**Choose Kck if you:**
- want to stay close to ASP.NET Core (DI, options, hosting conventions);
- want to add and remove modules one by one rather than ABP's "everything automatic" approach;
- avoid source-generation or runtime-proxy magic (debuggability matters);
- want a framework built for modern .NET 10 APIs (AOT, `Guid.CreateVersion7()`);
- value ADRs, per-provider guides and a zero-warning build.

**Choose something else if you:**
- want one package that covers everything → ABP;
- only write HTTP endpoint APIs → FastEndpoints is lighter;
- need microservice orchestration → Aspire (it also works together with Kck).

## Modules

### Abstractions

| Package | Responsibility |
|---|---|
| `Kck.Core.Abstractions` | Core entities, paging, result types |
| `Kck.Authorization.Abstractions` | Permission / policy contracts |
| `Kck.BackgroundJobs.Abstractions` | Job scheduling contracts |
| `Kck.Caching.Abstractions` | Distributed cache contracts |
| `Kck.Documents.Abstractions` | PDF / Excel / image processing contracts |
| `Kck.EventBus.Abstractions` | Pub/sub event bus contracts |
| `Kck.Exceptions.Abstractions` | Domain exception types |
| `Kck.FeatureFlags.Abstractions` | Feature flag contracts |
| `Kck.FileStorage.Abstractions` | FTP / blob storage contracts |
| `Kck.Http.Abstractions` | Resilient HTTP client contracts |
| `Kck.Localization.Abstractions` | i18n / l10n contracts |
| `Kck.Messaging.Abstractions` | Email / SMS contracts |
| `Kck.Observability.Abstractions` | Tracing / metrics / health contracts |
| `Kck.Persistence.Abstractions` | Repository, UoW, query contracts |
| `Kck.Search.Abstractions` | Full-text search contracts |
| `Kck.Security.Abstractions` | Hashing, JWT, TOTP, secrets contracts |

### Providers

| Area | Packages |
|---|---|
| Background Jobs | `Kck.BackgroundJobs.Hangfire`, `Kck.BackgroundJobs.Quartz` |
| Caching | `Kck.Caching.InMemory`, `Kck.Caching.Redis`, `Kck.Caching.Hybrid` |
| Documents | `Kck.Documents.ClosedXml`, `Kck.Documents.ImageSharp` |
| Event Bus | `Kck.EventBus.InMemory`, `Kck.EventBus.RabbitMq`, `Kck.EventBus.AzureServiceBus` |
| Exceptions | `Kck.Exceptions.AspNetCore` |
| Feature Flags | `Kck.FeatureFlags.InMemory` |
| File Storage | `Kck.FileStorage.FluentFtp` |
| HTTP | `Kck.Http.Resilience` |
| Hosting | `Kck.Hosting.Aspire` |
| Resilience | `Kck.Resilience.Polly` |
| Localization | `Kck.Localization`, `Kck.Localization.Json`, `Kck.Localization.Yaml` |
| Logging | `Kck.Logging.Serilog` |
| Messaging | `Kck.Messaging.MailKit`, `Kck.Messaging.SendGrid`, `Kck.Messaging.AmazonSes` |
| Observability | `Kck.Observability.OpenTelemetry` |
| Persistence | `Kck.Persistence.EntityFramework` |
| Pipeline | `Kck.Pipeline.Mediator` |
| Search | `Kck.Search.Elasticsearch` |
| Security | `Kck.Security.Argon2`, `Kck.Security.Jwt`, `Kck.Security.Totp`, `Kck.Security.TokenBlacklist.Redis`, `Kck.Security.Secrets.UserSecrets`, `Kck.Security.Secrets.AzureKeyVault` |
| Web | `Kck.AspNetCore` |

> **Background jobs:** Hangfire and Quartz are **alternatives**; using both is not recommended. Choose Hangfire if you
> need its dashboard and persistent storage, Quartz for lighter cron scheduling with DI integration. Both together
> enlarge your dependencies for nothing (~5 MB+ of assemblies).

### Bundles

| Bundle | Contents |
|---|---|
| `Kck.Bundle.WebApi` | ASP.NET Core defaults: Serilog, ProblemDetails exception handling, in-memory caching, in-memory event bus, Argon2, health checks, rate limiting, security headers, CORS from configuration; JWT token service when `Kck:Security:Jwt` is set |
| `Kck.Bundle.MinimalApi` | Slim ASP.NET Core stack: exception handler, caching, JWT, Serilog, OpenTelemetry |
| `Kck.Bundle.WorkerService` | Worker/daemon stack: Serilog, OpenTelemetry, in-memory event bus, Quartz |

## Samples

- `dotnet new kck-webapi` — the fastest start: an API solution with persistence, domain events, JWT and tests
  ([Kck.Templates](https://github.com/omerkck41/OmerkckArchitecture/blob/main/src/Kck.Templates/README.md))
- [`samples/Kck.Sample.Orders`](https://github.com/omerkck41/OmerkckArchitecture/tree/main/samples/Kck.Sample.Orders) — reference
  app: orders, soft delete, domain and integration events, dynamic queries, caching, OpenTelemetry; tested on SQLite
  and PostgreSQL
- [`samples/Kck.Sample.MinimalApi`](https://github.com/omerkck41/OmerkckArchitecture/tree/main/samples/Kck.Sample.MinimalApi) — Minimal API sample
- [`samples/Kck.Sample.WorkerService`](https://github.com/omerkck41/OmerkckArchitecture/tree/main/samples/Kck.Sample.WorkerService) — Quartz hosted jobs

## Testing

```bash
# Unit tests (no Docker needed; Windows and Linux):
dotnet test --filter "Category!=Integration"

# Integration tests (Docker needed):
dotnet test --filter "Category=Integration"
```

More: [`docs/test-strategy.md`](docs/test-strategy.md) and [`docs/policies/test-coverage.md`](docs/policies/test-coverage.md)

## Documentation

- [Documentation index](docs/README.md) — landing page
- [Architecture Decision Records](docs/adr/README.md) — binding technical decisions
- [Provider guides](docs/providers/README.md) — per-category usage notes (17 guides)

## Development

```bash
dotnet restore
dotnet build -c Release
dotnet test -c Release --no-build
```

**Requirements:** .NET 10 SDK

### Benchmarks

```bash
dotnet run -c Release --project tests/Kck.Benchmarks
```

Run a single benchmark with BenchmarkDotNet's `--filter`:

```bash
dotnet run -c Release --project tests/Kck.Benchmarks -- --filter "*Paginate*"
```

> Results are meaningful only in Release; a Debug run prints a warning.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Issues and PRs are welcome.

## License

[MIT](https://github.com/omerkck41/OmerkckArchitecture/blob/main/LICENSE) © Omerkck
