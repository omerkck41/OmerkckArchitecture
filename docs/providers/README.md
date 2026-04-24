# Provider Rehberleri

Bu dizin, her provider'in kullanimi, konfigurasyonu ve yaygin kullanim
orneklerini acIklar. Her rehber kategoriye gore gruplandirilmistir.

## Kategoriler

| Kategori | Rehber | Paketler |
|---|---|---|
| Background Jobs | [background-jobs.md](background-jobs.md) | Hangfire, Quartz |
| Caching | [caching.md](caching.md) | InMemory, Redis |
| Documents | [documents.md](documents.md) | ClosedXml, ImageSharp |
| Event Bus | [event-bus.md](event-bus.md) | InMemory, RabbitMq, AzureServiceBus |
| Exceptions | [exceptions.md](exceptions.md) | AspNetCore |
| Feature Flags | [feature-flags.md](feature-flags.md) | InMemory |
| File Storage | [file-storage.md](file-storage.md) | FluentFtp |
| HTTP | [http.md](http.md) | Resilience |
| Localization | [localization.md](localization.md) | Core, Json, Yaml |
| Logging | [logging.md](logging.md) | Serilog |
| Messaging | [messaging.md](messaging.md) | MailKit, SendGrid, AmazonSes |
| Observability | [observability.md](observability.md) | OpenTelemetry |
| Persistence | [persistence.md](persistence.md) | EntityFramework |
| Pipeline | [pipeline.md](pipeline.md) | MediatR |
| Search | [search.md](search.md) | Elasticsearch |
| Security | [security.md](security.md) | Argon2, Jwt, Totp, TokenBlacklist, Secrets |
| Web | [aspnetcore.md](aspnetcore.md) | AspNetCore |

## Ortak Yapi

Her provider su kalibi takip eder:

1. **Abstraction** — `Kck.<Area>.Abstractions` arayuz ve kontrat tanimlar
2. **Provider** — `Kck.<Area>.<Tech>` abstraction'i somut teknolojiyle karsilar
3. **DI Extension** — `AddKck<Area><Tech>()` tek cagiriyla kaydeder
4. **Options** — `IOptionsMonitor<T>` uzerinden konfigurasyon

## Abstraction Degistirme

Aym abstraction'a birden fazla provider eklenebilir (ornek: Caching.InMemory →
Caching.Redis). `TryAddSingleton` tutarliligi sayesinde ilk kaydedilen kazanir;
farkli provider'a gecmek icin registration sirasini degistirin veya
`services.Remove()` kullanin — detay: [ADR-0009](../adr/0009-tryaddsingleton-consistency.md).

## Konfigurasyon

Tum provider'lar `IOptionsMonitor<T>` kullanir — runtime reload destegi icin
tercih edildi ([ADR-0004](../adr/0004-ioptions-monitor-migration.md)).

```csharp
services.AddKckCachingRedis(opt =>
{
    opt.ConnectionString = builder.Configuration["Redis:ConnectionString"]!;
    opt.KeyPrefix = "myapp:";
});
```
