# Migration v3.x → v4.0

**Impact:** Breaking · **Estimated effort:** S–M (most changes show up as compiler errors)
**Decision:** [ADR-0028](../adr/0028-v4-scope.md)

## Why this major?

APIs announced as `[Obsolete]` during 3.x were removed, the default usage path now relies entirely on permissively
licensed packages (MediatR and the bundled Hangfire were dropped), and major dependencies were updated. The last 3.x
release stays listed on nuget.org; 3.x only receives security and critical fixes (`release/3.x`).

## At a glance

| Package | Change | What to do |
|---|---|---|
| `Kck.Pipeline.MediatR` | **Removed** (MediatR 13+ is commercial) | Move to `Kck.Pipeline.Mediator` — [guide](mediatR-to-mediator.md) |
| `Kck.Persistence.*` | `IReadRepository` methods with bool flags removed (KCK0100) | Use `QueryOptions` (table below) |
| `Kck.Security.*` | `GetClaimsFromToken` removed (KCK0001) | `ValidateTokenAsync` |
| `Kck.EventBus.InMemory` | `AddKckEventBusInMemory` removed | `AddKckEventBus(b => b.UseInMemory())` |
| `Kck.Bundle.WorkerService` | Hangfire removed, Quartz is the default | Add the Hangfire package yourself if you need it |
| `Kck.Documents.ImageSharp` | Unknown format throws; empty format keeps the source format | Check your format names |
| `Kck.BackgroundJobs.Quartz` | Quartz.NET 4.6 | `Action<IQuartzBuilder>` |
| `Kck.Caching.Hybrid` | `IDistributedCache` removed from the `HybridCacheService` constructor | No change if you use DI |
| `Kck.Caching.Redis`, `.Hybrid`, `TokenBlacklist.Redis` | StackExchange.Redis 3 (RESP3 by default) | See the notes below |

## 1. MediatR

`Kck.Pipeline.MediatR` is not published in 4.0. The 3.x package is marked "deprecated" on nuget.org. Migration:
[mediatR-to-mediator.md](mediatR-to-mediator.md).

## 2. Repository read methods (KCK0100)

In 3.x, the `QueryOptions` extension methods are now members of `IReadRepository<T, TId>`; the overloads with bool
flags and `ReadRepositoryExtensions` were removed. If you do not pass `options`, the 3.x defaults are preserved
(Get/GetList/GetListByDynamic track entities; Any/GetById/Count do not).

| 3.x | 4.0 |
|---|---|
| `repo.GetAsync(p, QueryOptions.Tracking)` (extension) | same — no change |
| `repo.GetAsync(p)` / `repo.GetListAsync(size: 10)` | same (tracking on) |
| `repo.GetAsync(p, withDeleted: true)` | `repo.GetAsync(p, new QueryOptions(IncludeDeleted: true, AsTracking: true))` |
| `repo.GetAsync(p, enableTracking: false)` | `repo.GetAsync(p, QueryOptions.None)` |
| `repo.GetByIdAsync(id, withDeleted: true)` | `repo.GetByIdAsync(id, QueryOptions.WithDeleted)` |
| `repo.AnyAsync(p)` / `repo.CountAsync(p)` / `repo.GetListAsync(p)` | `AnyAsync(predicate: p)` etc. — `options` is the first parameter |
| `ReadRepositoryExtensions.GetAsync(repo, ...)` | `repo.GetAsync(...)` |

> **Warning:** `QueryOptions.WithDeleted` does not track entities. In 3.x, `GetAsync(p, withDeleted: true)` did track;
> in a load-modify-`SaveChanges` flow, if you do not pass `AsTracking: true`, your changes are silently not saved.

If you have your own `IReadRepository` implementation, implement the new signatures.

## 3. JWT: `GetClaimsFromToken` (KCK0001)

It read claims without verifying the signature. Use `ValidateTokenAsync` for authorization:

<!-- snippet: skip (before: removed in 4.0) -->
```csharp
// Before (3.x)
var claims = tokenService.GetClaimsFromToken(token);
```

```csharp
// After (4.0)
using Kck.Security.Abstractions.Token;

public sealed class TokenCheck(ITokenService tokenService)
{
    public async Task<bool> IsValidAsync(string token, CancellationToken ct)
    {
        var result = await tokenService.ValidateTokenAsync(token, ct);
        return result.IsValid; // claims: result.UserId, result.Roles, result.Claims
    }
}
```

If you only need an unsigned read for logging/debugging, use `Microsoft.IdentityModel.JsonWebTokens.JsonWebToken`
yourself; do not use that value for authorization decisions.

## 4. InMemory event bus

<!-- snippet: skip (before: removed in 4.0) -->
```csharp
// Before (3.x)
services.AddKckEventBusInMemory(typeof(Program).Assembly);
```

```csharp
// After (4.0)
services.AddKckEventBus(b => b
    .UseInMemory()
    .RegisterHandlersFromAssembly(typeof(Program).Assembly));
```

## 5. Worker Service bundle

The bundle uses only Quartz, and Quartz is the default. The 3.x default was Hangfire:

- If `"Kck:BackgroundJobs:Provider"` is not set in your configuration, the application now runs with **Quartz**. If you
  had scheduled jobs on Hangfire in 3.x, they are not moved out of the Hangfire storage.
- `"Provider": "Hangfire"` throws `InvalidOperationException` at startup. To stay on Hangfire: add the
  `Kck.BackgroundJobs.Hangfire` package, call `AddKckBackgroundJobsHangfire(...)`, and register
  Serilog/OpenTelemetry/event bus yourself instead of using the bundle.
- The `BackgroundJobs:HangfireWorkerCount` setting was removed.

## 6. ImageSharp format rules

| Input | 3.x | 4.0 |
|---|---|---|
| `avif`, a typo, etc. | silently JPEG | `NotSupportedException` |
| empty / `null` format | JPEG | format of the source image (JPEG if the source is not supported) |
| `jpg` | JPEG | JPEG |

## 7. Quartz.NET 4

`AddKckBackgroundJobsQuartz(Action<IServiceCollectionQuartzConfigurator>?)` →
`AddKckBackgroundJobsQuartz(Action<IQuartzBuilder>?)`. If you have your own `IJob` classes, the signature is now
`ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)`. Quartz 4 behavior
changes: cron triggers without a time zone use `DefaultTimeZone`, the misfire default is `DefaultMisfireHandling`,
jobs/triggers without an identity raise warning 4030, and a canceled run is recorded as `Cancelled`.
Details: [Quartz 4 migration guide](https://www.quartz-scheduler.net/documentation/quartz-4.x/migration-guide.html),
[decisions/2026-10-27-quartz-4.md](https://github.com/omerkck41/OmerkckArchitecture/blob/main/decisions/2026-10-27-quartz-4.md).

## 8. StackExchange.Redis 3 and OpenTelemetry 1.20

- **Redis:** RESP3 is the default protocol (use `ConfigurationOptions.Protocol` for RESP2); commands that require
  `AllowAdmin` (e.g. `FLUSHALL`) are rejected on the client unless `AllowAdmin` is set.
- **OpenTelemetry:** the OTLP exporter's own `HttpClient` does not follow 3xx redirects; the per-send upper limit is 64 MiB.

## 9. HybridCacheService

`HybridCacheService(HybridCache, IDistributedCache, IConnectionMultiplexer, IOptionsMonitor<HybridCacheOptions>)` →
`HybridCacheService(HybridCache, IConnectionMultiplexer, IOptionsMonitor<HybridCacheOptions>)`.
No change if you register it with `AddKckCachingHybrid(...)`.

## Checklist

- [ ] Upgrade the `Kck.*` packages to 4.0 and build; any remaining errors match one of the sections above.
- [ ] For `withDeleted: true` calls, check whether you need tracking (section 2).
- [ ] Worker Service: check the scheduler choice and persistent job data (section 5).
- [ ] Verify your Redis/OTLP setup against section 8.

Problems: [GitHub Issues](https://github.com/omerkck41/OmerkckArchitecture/issues).
