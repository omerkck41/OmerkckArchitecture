# Provider Guides

This directory explains the usage, configuration and common examples of each
provider. The guides are grouped by category.

## Categories

| Category | Guide | Packages |
|---|---|---|
| Background Jobs | [background-jobs.md](background-jobs.md) | Hangfire, Quartz |
| Caching | [caching.md](caching.md) | InMemory, Redis |
| Documents | [documents.md](documents.md) | ClosedXml, SkiaSharp, ImageSharp (removed in 5.0) |
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
| Pipeline | [pipeline.md](pipeline.md) | Mediator |
| Search | [search.md](search.md) | Elasticsearch |
| Security | [security.md](security.md) | Argon2, Jwt, Totp, TokenBlacklist, Secrets |
| Web | [aspnetcore.md](aspnetcore.md) | AspNetCore |

## Common Structure

Every provider follows this pattern:

1. **Abstraction**: `Kck.<Area>.Abstractions` defines the interfaces and contracts
2. **Provider**: `Kck.<Area>.<Tech>` implements the abstraction with a concrete technology
3. **DI Extension**: `AddKck<Area><Tech>()` registers it with a single call
4. **Options**: an options class configured in the registration call

## Switching Abstractions

You can add more than one provider for the same abstraction (for example Caching.InMemory →
Caching.Redis). Because of `TryAddSingleton` consistency, the first registered provider wins.
To switch to a different provider, change the registration order or use
`services.Remove()`. Details: [ADR-0009](../adr/0009-tryaddsingleton-consistency.md).

## Configuration

Most providers read their options through `IOptionsMonitor<T>`, chosen for runtime reload support
([ADR-0004](../adr/0004-ioptions-monitor-migration.md)). Some options are read once at registration and need a
restart to change, for example the Hangfire worker count and the HTTP resilience policy.

```csharp
services.AddKckCachingRedis(
    redis => redis.Configuration = builder.Configuration["Redis:ConnectionString"],
    cache => cache.KeyPrefix = "myapp:");
```
