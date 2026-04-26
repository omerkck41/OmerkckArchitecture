# Architecture Decision Records (ADR)

Bu dizin, projedeki kritik mimari kararlari belgeleyen ADR kayitlarini icerir.
Her ADR sayisal prefix ile siralanir, immutable kabul edilir — sonradan degisen
bir karar yeni ADR ile `superseded` isaretlenerek gecilir.

## Durum Ozeti

| ADR | Baslik | Durum | Tarih | Etki Alani |
|---|---|---|---|---|
| [0001](0001-argon2-implementation.md) | Argon2 implementasyonu — paket korundu | Onaylandi | 2026-04-20 | `Kck.Security.Argon2` |
| [0002](0002-hangfire-storage.md) | Hangfire storage sadelestirme (MySqlStorage kaldirildi) | Onaylandi | 2026-04-20 | `Kck.BackgroundJobs.Hangfire` |
| [0003](0003-jwt-handler-migration.md) | `JwtSecurityTokenHandler` → `JsonWebTokenHandler` | Onaylandi | 2026-04-20 | `Kck.Security.Jwt` |
| [0004](0004-ioptions-monitor-migration.md) | `IOptions<T>` → `IOptionsMonitor<T>` migrasyonu | Onaylandi | 2026-04-20 | Tum provider'lar |
| [0005](0005-ef-repository-factory.md) | `EfUnitOfWork` service locator → `IEfRepositoryFactory` | Onaylandi | 2026-04-20 | `Kck.Persistence.EntityFramework` |
| [0006](0006-redis-async-hosted-service.md) | Redis `ConnectAsync` + `IHostedService` | Onaylandi | 2026-04-20 | `Kck.Caching.Redis`, `Kck.Security.TokenBlacklist.Redis` |
| [0007](0007-add-kck-job-helper.md) | `AddKckJob<TJob>()` helper | Onaylandi | 2026-04-20 | `Kck.BackgroundJobs.*` |
| [0008](0008-exceptions-abstractions-split.md) | `Kck.Exceptions` → `Abstractions` split | Onaylandi | 2026-04-20 | `Kck.Exceptions.*` |
| [0009](0009-tryaddsingleton-consistency.md) | `TryAddSingleton` tutarliligi | Onaylandi | 2026-04-20 | 19 provider |
| [0010](0010-deprecation-policy.md) | Deprecation policy + DiagnosticId numaralandirmasi | Onaylandi | 2026-04-25 | Tum public API |
| [0011](0011-multi-target-net8-net10.md) | Multi-target net8.0 + net10.0 | Onaylandi | 2026-04-26 | Abstraction'lar + saf provider'lar |
| [0012](0012-remove-automapper-package.md) | AutoMapper paketi kaldirildi (kullanilmiyor) | Onaylandi | 2026-04-26 | `Directory.Packages.props` |
| [0013](0013-public-api-discipline.md) | Public API disiplini (PublicApiAnalyzers + SemVer) | Onaylandi | 2026-04-26 | 16 abstraction projesi |

## Kategori

### Guvenlik
- [0001](0001-argon2-implementation.md) — Parola hash paket karari
- [0003](0003-jwt-handler-migration.md) — Modern JWT handler

### Bagimlilik Riski
- [0002](0002-hangfire-storage.md) — Terk edilmis paket kaldirildi
- [0012](0012-remove-automapper-package.md) — Kullanilmayan paket kaldirildi

### Arayuz / DI Tasarimi
- [0004](0004-ioptions-monitor-migration.md) — Options reload semantigi
- [0005](0005-ef-repository-factory.md) — Service locator anti-pattern
- [0006](0006-redis-async-hosted-service.md) — Async lifecycle
- [0007](0007-add-kck-job-helper.md) — DI registration ergonomisi
- [0008](0008-exceptions-abstractions-split.md) — Abstraction leakage
- [0009](0009-tryaddsingleton-consistency.md) — Multi-registration davranisi

### Politika / Yonetisim
- [0010](0010-deprecation-policy.md) — Public API obsoletion lifecycle
- [0013](0013-public-api-discipline.md) — Public API tracking + SemVer

### Build / Hedef Framework
- [0011](0011-multi-target-net8-net10.md) — net8.0 LTS + net10.0 STS

## Yeni ADR Yazma

Sablon: `../adr-template.md` (veya asagidaki yapi):

```markdown
# ADR-NNNN: [Karar Basligi]
Tarih: YYYY-MM-DD
Durum: Taslak | Onaylandi | Reddedildi | Superseded

## Baglam
Neden bu karara ihtiyac var?

## Karar
Ne yapilacak?

## Alternatifler Degerlendirildi
Neden bu secildi, digerleri neden elendi?

## Sonuclar
Bu kararin olumlu ve olumsuz etkileri.
```

**Zorunlu tetikleyiciler:** yeni framework, major versiyon atlamasi, mimari desen
degisimi, DB degisimi, auth sistemi degisimi, yeni MCP/harici servis.
