# Kck Modular Architecture Framework

[![Build](https://img.shields.io/github/actions/workflow/status/omerkck41/OmerkckArchitecture/build-test.yml?branch=main)](https://github.com/omerkck41/OmerkckArchitecture/actions/workflows/build-test.yml)
[![NuGet](https://img.shields.io/nuget/v/Kck.Core.Abstractions.svg)](https://www.nuget.org/packages/Kck.Core.Abstractions/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)

Modular, extensible .NET 10 framework organized around an **Abstractions → Providers → Bundles** pattern. Abstractions declare contracts; providers implement them against concrete technologies; bundles compose opinionated defaults for specific hosting models (WebApi, WorkerService, etc.).

## Quick Start

```bash
dotnet add package Kck.Bundle.WebApi
```

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddKckWebApi()
    .AddKckCachingInMemory()
    .AddKckMessagingMailKit(opt =>
    {
        opt.Host = builder.Configuration["Mail:Host"]!;
        opt.Port = 587;
        opt.UseSsl = true;
    });

var app = builder.Build();
app.UseKckExceptionHandler();
app.Run();
```

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
| Caching | `Kck.Caching.InMemory`, `Kck.Caching.Redis` |
| Documents | `Kck.Documents.ClosedXml`, `Kck.Documents.ImageSharp` |
| Event Bus | `Kck.EventBus.InMemory`, `Kck.EventBus.RabbitMq`, `Kck.EventBus.AzureServiceBus` |
| Exceptions | `Kck.Exceptions.AspNetCore` |
| Feature Flags | `Kck.FeatureFlags.InMemory` |
| File Storage | `Kck.FileStorage.FluentFtp` |
| HTTP | `Kck.Http.Resilience` |
| Localization | `Kck.Localization`, `Kck.Localization.Json`, `Kck.Localization.Yaml` |
| Logging | `Kck.Logging.Serilog` |
| Messaging | `Kck.Messaging.MailKit`, `Kck.Messaging.SendGrid`, `Kck.Messaging.AmazonSes` |
| Observability | `Kck.Observability.OpenTelemetry` |
| Persistence | `Kck.Persistence.EntityFramework` |
| Pipeline | `Kck.Pipeline.MediatR` |
| Search | `Kck.Search.Elasticsearch` |
| Security | `Kck.Security.Argon2`, `Kck.Security.Jwt`, `Kck.Security.Totp`, `Kck.Security.TokenBlacklist.Redis`, `Kck.Security.Secrets.UserSecrets`, `Kck.Security.Secrets.AzureKeyVault` |
| Web | `Kck.AspNetCore` |

### Bundles

| Bundle | Contents |
|---|---|
| `Kck.Bundle.WebApi` | Opinionated ASP.NET Core stack: MediatR pipeline, exception handler, Serilog, OpenTelemetry, JWT |

## Samples

- [`samples/Kck.Sample.WebApi`](samples/Kck.Sample.WebApi) — Full WebApi sample with RabbitMQ, Redis, EF Core
- [`samples/Kck.Sample.MinimalApi`](samples/Kck.Sample.MinimalApi) — Minimal API with Bundle.WebApi
- [`samples/Kck.Sample.WorkerService`](samples/Kck.Sample.WorkerService) — Hangfire + Quartz hosted jobs

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

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Issues and PRs are welcome.

## License

[MIT](LICENSE) © Omerkck
