# OmerkckArchitecture — Aksiyon Listesi

**Kaynak:** `tasks/audit-report-2026-04-20.md`
**Plan:** `tasks/audit-plan-2026-04-20.md`
**Son guncelleme:** 2026-04-24

> Durum: FAZ 0-7 tamamlandi. Detay: `tasks/checkpoint.md`.
> Ana fazlar yesil; asagidaki listede [x] = kapali, [ ] = opsiyonel/ertelenmis.

## P1 — Hemen (Faz 0 + Faz 1)

### Faz 0 — Altyapi (CRITICAL Infra)
- [x] `.gitignore` — .NET + VS/Rider/VSCode + test/coverage + local env
- [x] `README.md` — badge placeholder, modul tablosu, quick start, samples, docs
- [x] `LICENSE` (MIT) — 2026 Omerkck
- [x] `.editorconfig` — .NET 10 standart + naming + C# style
- [x] `.gitattributes` — LF default, sln CRLF, ps1/cmd CRLF, binary handling
- [x] `.github/workflows/build-test.yml` — ubuntu + windows matrix, test + coverage artifact
- [x] `.github/workflows/codeql.yml` — haftalik cron + push/PR
- [x] `.github/workflows/nuget-publish.yml` — tag-trigger + workflow_dispatch
- [x] `.github/dependabot.yml` — haftalik nuget + github-actions, gruplu
- [x] `CHANGELOG.md` — Keep a Changelog, [Unreleased] + FAZ 1-4 ozeti
- [x] `CONTRIBUTING.md` — Conventional Commits + branch strategy + PR checklist
- [x] `CODEOWNERS` — `* @omerkck` + ADR/CI koruma
- [x] `.env.example` — tum provider ornek anahtarlari
- [x] `.config/dotnet-tools.json` — minver + reportgenerator + dotnet-ef

### Faz 1 — Kritik Kod Duzeltmeleri
- [x] `JwtTokenService.GetClaimsFromToken` → `[Obsolete]` (KCK0001)
- [x] Samples `?? "guest"` hardcoded kaldir
- [x] `AddEntityFrameworkCoreInstrumentation()` ekle
- [x] `CacheServiceBase` semaphore eviction race duzelt (eviction kaldirildi)
- [x] 3 EventBus provider `OperationCanceledException` pattern (InMemory/RabbitMQ/Azure)
- [x] `FluentFtp/PathSanitizer` URL-encoded traversal kontrolu

## P2 — Bu Sprint (Faz 2 + Faz 3 + Faz 4 + Faz 5)

### Faz 2 — Bagimlilik
- [x] `Konscious.Argon2` alternatif arastir + ADR (ADR-0001: korundu, gerekce: migration riski)
- [x] `Hangfire.MySqlStorage` kaldir + ADR (ADR-0002)
- [x] `System.IdentityModel.Tokens.Jwt` → `JsonWebTokenHandler` (ADR-0003)
- [x] MailKit 4.15.1 → 4.16.0 (CVE GHSA-9j88-vvj5-vhgr)
- [x] `dependabot.yml` ekle

### Faz 3 — DI (BREAKING)
- [x] `IOptionsMonitor<T>` gecisi (ADR-0004)
- [x] `EfUnitOfWork` Service Locator → `IRepositoryFactory<T,TId>` (ADR-0005)
- [x] `Kck.Caching.Redis` `ConnectAsync` + `IHostedService` (ADR-0006)
- [x] `AddKckJob<TJob>()` helper (Hangfire + Quartz) (ADR-0007)
- [x] `Kck.Exceptions` → `Kck.Exceptions.Abstractions` ayir (ADR-0008)
- [x] `TryAddSingleton` tutarliligi (ADR-0009)

### Faz 4 — Hata + Performans
- [x] `FluentFtpService` connection pool — `FtpConnectionPool`
- [x] `EfRepository.DeleteRangeAsync` tek-geciste RemoveRange
- [x] `EfRepository` `QueryOptions` record + overload
- [x] `Paginate<T>.Create` factory + sync ctor kaldirildi
- [x] `SmtpConnectionPool._disposed` `Interlocked.CompareExchange`
- [x] `RedisCacheService.RemoveByPrefixAsync` 1000 key/batch chunked
- [ ] (Opsiyonel) Outbox pattern yeni provider — ertelendi

### Faz 5 — Test
- [x] 9 eksik provider test projesi (81 test, hepsi yesil)
  - [x] `Kck.Logging.Serilog.Tests` (13)
  - [x] `Kck.Security.Secrets.UserSecrets.Tests` (15)
  - [x] `Kck.Security.Secrets.AzureKeyVault.Tests` (6)
  - [x] `Kck.BackgroundJobs.Quartz.Tests` (7)
  - [x] `Kck.Documents.ImageSharp.Tests` (12)
  - [x] `Kck.Messaging.MailKit.Tests` (8)
  - [x] `Kck.Messaging.AmazonSes.Tests` (5)
  - [x] `Kck.Messaging.SendGrid.Tests` (4)
  - [x] `Kck.FeatureFlags.InMemory.Tests` (11)
- [x] Coverlet CI entegrasyonu (coverage artifact)
- [ ] Coverage threshold (CI gate) — eklendi 2026-04-24
- [ ] `FakeTimeProvider` ile flaky testleri duzelt (Cache, Jwt)
- [ ] `HangfireJobScheduler` gercek test senaryolari

## P3 — Sonra (Faz 6 + Faz 7)

### Faz 6 — Dokuman
- [x] `docs/adr/` index + 9 ADR
- [x] `docs/providers/` — 17 kategori rehberi
- [x] `docs/README.md` root navigasyon
- [x] Kok `README.md` docs bolumu
- [ ] Public API XML doc eksikleri (CS1591 NoWarn'da, ayri faz)
- [ ] `CHANGELOG.md` semantic release otomasyonu

### Faz 7 — Temizlik
- [x] `WarningsNotAsErrors` listesi 10 → 2 (`CA1716;CA1000`)
- [x] CA1848 — `[LoggerMessage]` partial (28 cagri, 18 dosya)
- [x] CA1305/CA1304 — `CultureInfo.InvariantCulture`
- [x] CA2016 — `CancellationToken` yonlendirmesi
- [x] CA1001/CA1816 — `IDisposable` + `GC.SuppressFinalize`
- [x] CA1852 — `sealed record`
- [x] Build: `0 Warning(s) 0 Error(s)`

### Opsiyonel Polish (FAZ 7 orijinal plan artiklari)
- [ ] Serilog PII maskeleme Enricher
- [ ] `AmazonSesEmailProvider` tek foreach
- [ ] `LocalizationService` immutable
- [ ] `ElasticsearchSearchService` `IDisposable`
- [ ] `QuartzJobScheduler` `Lazy<IScheduler>`
- [ ] `DateTime.UtcNow` → `DateTimeOffset.UtcNow` sweep
- [ ] Collection expression `[...]` yayginlastir

---

**Aktif odak:** Coverage threshold (CI gate) — sprint sonrasi bakim ve polish.
**Ertelenmis:** Outbox pattern, Public API XML doc, FakeTimeProvider migrasyonu.
