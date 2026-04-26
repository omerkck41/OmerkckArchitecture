# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added (LS-FAZ-5)
- `Filter` API: `Filter(string field, FilterOperator op, ...)` constructor (enum'u canonical lower-case wire string'e cevirir), `OperatorEnum` getter, `TryParseOperator` static helper. Wire format degismedi — JSON consumer'lar etkilenmez (Library Strategy §4.4).
- `Filter.GetValue<T>` AOT-uyumlu rewrite: `Convert.ChangeType` yerine switch + tip-spesifik `ParseString<T>`. `JsonElement` artik native `Deserialize<T>` ile islenir; `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `Enum` desteklenir; bilinmeyen tipler icin `Convert.ChangeType` fallback (Library Strategy §5.5).
- `docs/adr/0015-perf-quickwins.md` — Redis EXISTS + Filter type safety + AOT karari.

### Changed (LS-FAZ-5)
- **Performance:** `RedisCacheService.ExistsAsync` artik `IConnectionMultiplexer.GetDatabase().KeyExistsAsync()` (sadece `EXISTS` komutu) kullaniyor — eski `IDistributedCache.GetStringAsync()` butun degeri network'ten cekiyordu. Buyuk-degerli cache'lerde %95+ network/GC azalma (Library Strategy §5.6, P1).
- `DynamicFilterExtensions.ApplyFilter` artik `filter.OperatorEnum ?? throw` kullaniyor — eskiden her cagride `Enum.Parse<FilterOperator>(...)` calistiriyordu.

### Added (LS-FAZ-4)
- `tests/Kck.Benchmarks` — BenchmarkDotNet 0.15.0 console app + 3 baslangic benchmark (`PaginateCreateBenchmarks`, `ResultBenchmarks`, `JsonSerializationBenchmarks`). LS-FAZ-5/6 perf optimizasyonlari icin baseline (Library Strategy §5.7).
- `tests/Kck.Persistence.EntityFramework.Tests/EfRepositoryIntegrationTests.cs` — Testcontainers PoC (postgres:16-alpine), `[Trait("Category", "Integration")]`. Mock/InMemory'nin kacirdigi gercek-DB davranislarini yakalar (Library Strategy §7.5).
- `Testcontainers.PostgreSql 4.5.0` + `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0` -> `Directory.Packages.props` (yalniz EF.Tests projesinde kullaniliyor).
- `docs/test-strategy.md` — test piramidi rehberi (unit vs integration vs benchmark, ne zaman hangisi, mevcut coverage matrix).
- `docs/policies/test-coverage.md` — coverage gate politikasi + kademeli yukseltme yol haritasi (40/35 -> 50/45 -> 65/50 -> 75/60).
- `docs/adr/0014-test-strategy.md` — BenchmarkDotNet + Testcontainers + coverage policy karari.
- README "Testing" + "Benchmarks" bolumleri (Docker gerektiren / gerektirmeyen test komutlari).

### Changed (LS-FAZ-4)
- CI `build-test.yml` test step ikiye boldu: `Test (unit)` (filter `Category!=Integration`, hem ubuntu hem windows) ve `Test (integration)` (filter `Category=Integration`, sadece ubuntu — Docker gerekli).
- CI coverage gate: `BRANCH_THRESHOLD: 30 -> 35` (regression onleme; gercek olcum %36.3, 1.3 puan pay). Line gate sabit %40 (gercek %40.7).

### Added (LS-FAZ-3)
- `Microsoft.CodeAnalysis.PublicApiAnalyzers 4.14.0` — 16 abstraction projesinde aktif (ADR-0013). Yeni public API ekleyenler `PublicAPI.Unshipped.txt`'a satir eklemek zorunda; aksi halde RS0016 ile build hatasi.
- `PublicAPI.Shipped.txt` baseline tum abstraction'larda — 875 unique public symbol kayit altinda.
- `src/Abstractions/Directory.Build.props` — analyzer'i tum 16 abstraction'a tek noktadan uygular (root Directory.Build.props re-import).
- `docs/policies/support.md` — .NET destek matrisi (LTS/STS), TFM ekleme/kaldirma akisi (Library Strategy §6.4).
- `docs/policies/versioning.md` — SemVer kontrati, "Public API" tanimi, breaking change politikasi, PublicApiAnalyzers ile tracking sureci (Library Strategy §12.3).
- `docs/migrations/README.md` + `_template.md` — per-major migration rehber sablonu (Library Strategy §9.4).
- `docs/adr/0013-public-api-discipline.md` — PublicApiAnalyzers + SemVer + Obsolete disiplini karari.
- `[DebuggerDisplay]` — `Result`, `Result<T>`, `Error`, `Paginate<T>`, `PageRequest` (Library Strategy §9.3); watch window'da class adi yerine anlamli ozet.
- `docs/adr/README.md` indexine ADR-0011/0012/0013 eklendi (LS-FAZ-2'den geri kalan toparlama).

### Changed (LS-FAZ-3)
- `Kck.Caching.Redis` eksik konfigurasyon hata mesaji "pit-of-success" formatinda yeniden yazildi: 3 alternatifin somut ornegi + dokuman linki (Library Strategy §9.2 literal ornek).

### Added (LS-FAZ-2)
- Multi-target build: `Kck.*.Abstractions` + saf provider'lar artik `net8.0` LTS + `net10.0` STS hedefli (ADR-0011). Net10-only opt-out paketler: `Kck.Bundle.WebApi`, `Kck.Persistence.EntityFramework`, `Kck.AspNetCore`, `Kck.Security.Jwt`, `Kck.Caching.Redis`, `Kck.Http.Resilience`, `Kck.Exceptions.AspNetCore`.
- `docs/adr/0011-multi-target-net8-net10.md` — multi-target karari ve net8 fallback tablosu.
- `docs/adr/0012-remove-automapper-package.md` — kullanilmayan AutoMapper paketinin kaldirilmasi karari.
- README "Quick Start" 3-sekme: WebApi (Bundle), MinimalApi (slim), WorkerService (no HTTP).
- README "Why Kck?" karsilastirma bolumu (vs ABP / FastEndpoints / Aspire).
- README Background Jobs ipucu: Hangfire ya da Quartz, ikisi birden onerilmez.

### Changed (LS-FAZ-2)
- `Microsoft.IdentityModel.JsonWebTokens 8.17.0` pin'i yorumla netlestirildi — diamond-dependency hizalamasi (Library Strategy §2.3 madde 2).
- Net8 fallback'leri eklendi (`#if NET9_0_OR_GREATER`): `IntegrationEvent.Id`, `QuartzJobScheduler.NewJobId()`, `TotpMfaProvider` secret hash. Net8'de `Guid.NewGuid()` ve `Convert.ToHexString().ToLowerInvariant()` kullanilir; net9+ surumlerde sirasiyla `Guid.CreateVersion7()` ve `Convert.ToHexStringLower()`.

### Removed (LS-FAZ-2)
- `AutoMapper 16.1.1` PackageVersion girisi `Directory.Packages.props`'tan kaldirildi (sifir kullanim, ADR-0012).

### Added
- `SECURITY.md` — security policy with CVSS-based response SLA and 90-day coordinated disclosure (Library Strategy §13.1).
- `docs/policies/deprecation.md` — formal deprecation policy with `KCK0001-0999` DiagnosticId numbering (ADR-0010).
- `docs/adr/0010-deprecation-policy.md` — records the obsoletion lifecycle decision.
- Source Link & deterministic build enabled via `Microsoft.SourceLink.GitHub 8.0.0`; `.snupkg` symbol packages now produced (Library Strategy §6.1).
- `.github/workflows/license-audit.yml` — weekly NuGet license scan; blocks GPL/AGPL/LGPL/SSPL/BUSL contamination (Library Strategy §13.6).
- CI: `dotnet list package --vulnerable --include-transitive` step; Critical/High CVE → fail (Library Strategy §13.2).
- `FtpConnectionPool` for `Kck.FileStorage.FluentFtp` — pooled `AsyncFtpClient` connections with configurable `PoolSize`.
- `QueryOptions` readonly record struct in `Kck.Persistence.Abstractions` — opt-in replacement for positional `bool withDeleted / enableTracking` flags.
- `Paginate<T>.Create` static factory that replaces the removed internal synchronous constructor.
- `AddKckJob<TJob>()` helper for Hangfire and Quartz providers.
- `Kck.Exceptions.Abstractions` package split out from `Kck.Exceptions`.
- ADRs 0001–0009 under `docs/adr/`.

### Changed (LS-FAZ-1)
- `JwtTokenService.GetClaimsFromToken` `[Obsolete]` attribute now ships with `UrlFormat` pointing to `docs/policies/deprecation.md` (KCK0001).
- `nuget-publish.yml` no longer passes `/p:ContinuousIntegrationBuild=true` — moved to `Directory.Build.props` under `GITHUB_ACTIONS` condition.

### Changed
- **BREAKING:** `IOptions<T>` → `IOptionsMonitor<T>` across all provider services (ADR-0004).
- **BREAKING:** `EfUnitOfWork` no longer uses service-locator pattern; replaced with `IEfRepositoryFactory` (ADR-0005).
- **BREAKING:** `Kck.Caching.Redis` registration now uses async `ConnectAsync` + `IHostedService` (ADR-0006).
- **BREAKING:** `FluentFtpService` constructor now depends on `FtpConnectionPool` instead of `IOptionsMonitor<FluentFtpOptions>`.
- `EfRepository.DeleteRangeAsync` rewritten from `foreach + await DeleteAsync` (N async allocations) to a single-pass state-setting loop.
- `RedisCacheService.RemoveByPrefixAsync` now chunks deletes into 1000-key batches.
- `SmtpConnectionPool._disposed` uses `Interlocked.CompareExchange` for thread-safe disposal.
- `TryAddSingleton` used consistently across provider DI registrations (19 files).
- `JwtTokenService` migrated to `JsonWebTokenHandler` (ADR-0003).
- `MailKit` upgraded to 4.16.0 (CVE GHSA-9j88-vvj5-vhgr).

### Removed
- `Hangfire.MySqlStorage` dependency (ADR-0002).
- `JwtTokenService.GetClaimsFromToken` marked `[Obsolete]` — does not verify signature.

### Security
- Samples no longer fall back to `"guest"` when RabbitMQ credentials are absent — fail fast instead.
- `OpenTelemetry.Instrumentation.EntityFrameworkCore` added for DB-call tracing.
- `CacheServiceBase` semaphore eviction race removed.
- `EventBus` providers (`InMemory`, `RabbitMq`, `AzureServiceBus`) now propagate `OperationCanceledException` correctly.
- `PathSanitizer` rejects URL-encoded (`%2e%2e`, `..%2f`) traversal attempts.

## [0.1.0] — Initial public cut

First public drop of the Kck modular architecture framework.
