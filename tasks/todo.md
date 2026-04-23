# OmerkckArchitecture — Aksiyon Listesi

**Kaynak:** `tasks/audit-report-2026-04-20.md`
**Plan:** `tasks/audit-plan-2026-04-20.md`
**Son guncelleme:** 2026-04-20

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
- [x] `Hangfire.MySqlStorage` kaldir + ADR (ADR-0002: referans yoktu, Directory.Packages.props'tan silindi)
- [x] `System.IdentityModel.Tokens.Jwt` → `JsonWebTokenHandler` (ADR-0003)
- [x] MailKit 4.15.1 → 4.16.0 (CVE GHSA-9j88-vvj5-vhgr; SmtpConnectionPool null-guard eklendi)
- [x] `dependabot.yml` ekle (FAZ 0 ile birlikte tamamlandi)

### Faz 3 — DI (BREAKING)
- [ ] `IOptionsMonitor<T>` gecisi (tum Options)
- [ ] `EfUnitOfWork` Service Locator → `IRepositoryFactory<T,TId>`
- [ ] `Kck.Caching.Redis` `ConnectAsync` + `IHostedService`
- [ ] `AddKckJob<TJob>()` helper (Hangfire + Quartz)
- [ ] `Kck.Exceptions` → `Kck.Exceptions.Abstractions` ayir
- [ ] `TryAddSingleton` tutarliligi

### Faz 4 — Hata + Performans
- [x] `FluentFtpService` connection pool — `FtpConnectionPool` (Channel-tabanli, SmtpConnectionPool paternine gore) + `FluentFtpOptions.PoolSize`
- [x] `EfRepository.DeleteRangeAsync` foreach+await N-alloc -> tek-geciste state set (RemoveRange + soft-delete). `ExecuteDeleteAsync` yerine RemoveRange: interceptor/tracker korunur
- [x] `EfRepository` `QueryOptions` record — minimum kapsam: Abstractions'a eklendi + `EfRepository.Query(QueryOptions, ...)` overload. Interface migrasyonu major bump'a ertelendi
- [x] `Paginate.cs` — kullanilmayan senkron internal ctor kaldirildi, `Paginate<T>.Create` factory eklendi, `ToPaginateAsync` bu factory'i kullanir
- [x] `SmtpConnectionPool._disposed` `bool -> int` + `Interlocked.CompareExchange` + `Volatile.Read`
- [x] `RedisCacheService.RemoveByPrefixAsync` — 1000 key/batch chunked delete (LOH allocation onleme)
- [ ] (Opsiyonel) Outbox pattern yeni provider — ATLA (plan "opsiyonel", ayri karar noktasi)

### Faz 5 — Test
- [ ] 9 eksik provider test projesi
  - [ ] `Kck.Logging.Serilog.Tests`
  - [ ] `Kck.Security.Secrets.UserSecrets.Tests`
  - [ ] `Kck.Security.Secrets.AzureKeyVault.Tests`
  - [ ] `Kck.BackgroundJobs.Quartz.Tests`
  - [ ] `Kck.Documents.ImageSharp.Tests`
  - [ ] `Kck.Messaging.MailKit.Tests`
  - [ ] `Kck.Messaging.AmazonSes.Tests`
  - [ ] `Kck.Messaging.SendGrid.Tests`
  - [ ] `Kck.FeatureFlags.InMemory.Tests`
- [ ] Coverlet + ReportGenerator CI
- [ ] `FakeTimeProvider` ile flaky testleri duzelt (Cache, Jwt)
- [ ] `HangfireJobScheduler` gercek test senaryolari

## P3 — Sonra (Faz 6 + Faz 7)

### Faz 6 — Dokuman
- [ ] `docs/adr/` + 6 ADR (Argon2, Hangfire, Options, EF, MediatR, JWT)
- [ ] `CHANGELOG.md` semantic release
- [ ] `docs/providers/*.md` modul rehberi
- [ ] Public API XML doc eksikleri

### Faz 7 — Temizlik
- [ ] `WarningsNotAsErrors` listesini daralt (10 kural)
- [ ] Serilog PII maskeleme Enricher
- [ ] `AmazonSesEmailProvider` tek foreach
- [ ] `LocalizationService` immutable
- [ ] `ElasticsearchSearchService` `IDisposable`
- [ ] `QuartzJobScheduler` `Lazy<IScheduler>`
- [ ] `DateTime.UtcNow` → `DateTimeOffset.UtcNow` sweep
- [ ] Collection expression `[...]` yayginlastir

---

**Onay bekleyen faz:** FAZ 3 (DI Breaking Changes) — FAZ 2 tamamlandi (4/4 adim, ADR-0001/0002/0003, solution build 0 error 82 pre-existing warning, tum testler yesil)
**Talimat:** `FAZ 3 DEVAM` yazarak baslat, `ATLA` ile bir sonraki faza gec, `DEGISTIR [detay]` ile adapt et.
**Not:** Commit/push kullanici onayini bekliyor — tum degisiklikler local. Faz 0 (GitHub infra) ve dependabot.yml ertelendi.
