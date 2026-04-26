# Kck Modular Architecture Framework

[![Build](https://img.shields.io/github/actions/workflow/status/omerkck41/OmerkckArchitecture/build-test.yml?branch=main)](https://github.com/omerkck41/OmerkckArchitecture/actions/workflows/build-test.yml)
[![NuGet](https://img.shields.io/nuget/v/Kck.Core.Abstractions.svg)](https://www.nuget.org/packages/Kck.Core.Abstractions/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-512BD4.svg)](https://dotnet.microsoft.com/)

Modular, extensible .NET framework organized around an **Abstractions → Providers → Bundles** pattern. Abstractions declare contracts; providers implement them against concrete technologies; bundles compose opinionated defaults for specific hosting models (WebApi, WorkerService, etc.).

## Quick Start

### Web API (ASP.NET Core, full Bundle)

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
app.MapGet("/", () => Results.Ok("Hello from Kck!"));
app.Run();
```

> Tam ornek: [`samples/Kck.Sample.WebApi`](samples/Kck.Sample.WebApi)

### Minimal API (Slim, only what you need)

```bash
dotnet add package Kck.AspNetCore
dotnet add package Kck.Caching.InMemory
dotnet add package Kck.Pipeline.MediatR
```

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddKckExceptions()
    .AddKckCachingInMemory()
    .AddKckMediatR(typeof(Program).Assembly);

var app = builder.Build();
app.MapPost("/users", async (CreateUserCommand cmd, IMediator mediator) =>
    Results.Ok(await mediator.Send(cmd)));
app.Run();
```

> Tam ornek: [`samples/Kck.Sample.MinimalApi`](samples/Kck.Sample.MinimalApi)

### Worker Service (background jobs, no HTTP)

```bash
dotnet add package Kck.BackgroundJobs.Quartz
dotnet add package Kck.Logging.Serilog
dotnet add package Kck.Observability.OpenTelemetry
```

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddKckSerilog(builder.Configuration)
    .AddKckOpenTelemetry()
    .AddKckQuartz(opt => opt.UseInMemoryStore())
    .AddKckJob<EmailDigestJob>(triggerCron: "0 0 9 * * ?");

await builder.Build().RunAsync();
```

> Tam ornek: [`samples/Kck.Sample.WorkerService`](samples/Kck.Sample.WorkerService)

### Target Frameworks

| Paket Sinifi | TFM |
|---|---|
| `Kck.*.Abstractions` ve saf provider'lar | `net8.0` + `net10.0` |
| `Kck.Bundle.WebApi`, `Kck.Persistence.EntityFramework`, `Kck.AspNetCore`, `Kck.Security.Jwt`, `Kck.Caching.Redis`, `Kck.Http.Resilience`, `Kck.Exceptions.AspNetCore` | `net10.0` (tek hedef) |

Net10-only paketler ASP.NET Core 10 / EF Core 10 framework reference'ina bagimli oldugundan multi-target degildir. Detay: [ADR-0011](docs/adr/0011-multi-target-net8-net10.md).

## Why Kck?

Kck, ABP'den **daha hafif**, FastEndpoints'ten **daha kapsamli**, Aspire'a **uyumlu** olmayi hedefler:

| Konu | Kck | ABP Framework | FastEndpoints | Microsoft .NET Aspire |
|---|---|---|---|---|
| **Lisans** | MIT | LGPL (Pro: ucretli) | MIT | MIT |
| **Yaklasim** | Modular, opt-in | Tum-in-bir, opinionated | Endpoint-only | Orchestration |
| **DI Konvansiyonu** | `Microsoft.Extensions.*` native | ABP DI extension'lari | `Microsoft.Extensions.*` | `Microsoft.Extensions.*` |
| **Source-gen / Proxy** | Yok (saf C#) | Cok (UoW interceptor, dynamic proxy) | Yok | Yok |
| **TFM Destegi** | net8 LTS + net10 STS | net8 + net9 | net8 + net9 + net10 | net8 + net9 + net10 |
| **AOT-uyumu** | Kismen (abstraction'lar) | Hayir | Evet | Evet |
| **Provider Sayisi** | 30+ resmi | 15+ resmi (Pro) | N/A | N/A (orchestration) |
| **Battery-included** | Opt-in modullerle | Tum-in-bir | Sadece HTTP | Yok |
| **Source-Link + Symbols** | Var | Var | Var | Var |
| **Ogrenme Egrisi** | Dusuk-Orta | Yuksek | Dusuk | Orta |

**Kck'i secmen gerekiyorsa eger:**
- ASP.NET Core'a sadik kalmak istiyorsan (DI, options, hosting konvansiyonlari)
- Modul modul ekleme/cikarma yetkisi istiyorsan (ABP'nin "her sey otomatik" yaklasimini istemeyen)
- Source-gen veya runtime proxy magic'inden kaciniyorsan (debug edilebilirlik onemliyse)
- net8 LTS + net10 STS multi-target istiyorsan
- 9+ ADR + 17 provider doc + 0 warning build hijiyenine deger veriyorsan

**Kck'i secmemen gerekiyorsa:**
- "Tek paket kurar tum ihtiyacim biter" istiyorsan → ABP
- Yalniz HTTP endpoint API yaziyorsan → FastEndpoints daha hafif
- Microservice orchestration istiyorsan → Aspire (Kck ile birlikte de calisir)

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

> **Background Jobs ipucu:** Hangfire ve Quartz **alternatif**tir, beraber kullanilmasi onerilmez. Hangfire dashboard ve persistent storage ihtiyaciniz varsa onu, daha hafif cron + DI entegrasyonu istiyorsaniz Quartz'i secin. Ikisi birden bagimliliklarinizi gereksiz sisirir (~5MB+ assembly farki).

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

**Requirements:** .NET 10 SDK (multi-target paketleri net8 icin de derler)

### Benchmarks

```bash
dotnet run -c Release --project tests/Kck.Benchmarks
```

BenchmarkDotNet `--filter` ile spesifik benchmark calistir:

```bash
dotnet run -c Release --project tests/Kck.Benchmarks -- --filter "*Paginate*"
```

> Sadece Release konfigurasyonunda anlamli sonuc verir; Debug uyari ile kosur.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Issues and PRs are welcome.

## License

[MIT](LICENSE) © Omerkck
