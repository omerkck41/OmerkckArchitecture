# OmerkckArchitecture — Audit Uygulama Plani

**Tarih:** 2026-04-20
**Kaynak rapor:** `tasks/audit-report-2026-04-20.md`
**Yaklasim:** Faz faz ilerleme. **Her faz oncesi kullanici onayi zorunlu.** Onaysiz faza gecilmez.

---

## Genel Kurallar

- Her fazin basinda faz planinin detayli adimlari kullaniciya sunulur.
- Kullanici `DEVAM` / `ATLA` / `DEGISTIR [detay]` seceneklerinden birini secer.
- Her faz sonunda: `/code-review` + `/verification-before-completion` calistirilir.
- Kritik guvenlik veya mimari degisikliklerde: `/adversarial-review` eklenir.
- Her faz kendi commit grubunda toplanir (conventional commits). Push kullanici onayi ile.
- Faz bitiminde `tasks/checkpoint.md` guncellenir (hangi adim tamam, hangi kaldi).

## Faz Haritasi

| Faz | Baslik | Sure | Risk | Onay Gate |
|---|---|---|---|---|
| 0 | Altyapi Temelleri (CRITICAL Infra) | 1-2 saat | Dusuk | **PENDING** |
| 1 | Kritik Guvenlik ve Kod Duzeltmeleri | 2-3 saat | Orta | PENDING |
| 2 | Bagimlilik Riski Azaltma | 2-4 saat | Yuksek | PENDING |
| 3 | DI Mimari Iyilestirmeleri | 4-6 saat | Yuksek (breaking) | PENDING |
| 4 | Hata Yonetimi ve Performans | 3-5 saat | Orta | PENDING |
| 5 | Test Kapsami Genisletme | 4-8 saat | Dusuk | PENDING |
| 6 | Dokuman ve ADR | 2-3 saat | Dusuk | PENDING |
| 7 | Analyzer ve Kod Temizligi | 2-3 saat | Dusuk | PENDING |

---

## FAZ 0 — Altyapi Temelleri (CRITICAL Infra)

**Hedef:** Projeyi temel NuGet kutuphane normlarina uyumlu hale getirmek. Hicbir kod dokunulmaz, sadece repo/infra dosyalari eklenir.

**Onay Gate:** Baslamadan once kullanici `FAZ 0 DEVAM` demeli.

**Adimlar:**

1. **`.gitignore`** — Standart Visual Studio + .NET sablonu
   - `bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`, `.env`, `appsettings.*.local.json`, `TestResults/`, `*.DotSettings.user`, `.idea/`, `packages/`, `artifacts/`
   - Dogrulama: `git status` sonrasi bin/obj temiz olmali

2. **`README.md`** — Ana dokuman
   - Badge placeholder (build, nuget, license)
   - Proje amaci (1 paragraf)
   - Modul listesi (Abstractions + Providers tablosu)
   - Quick start: `dotnet add package Kck.Bundle.WebApi` + minimum kod ornegi
   - Docs link, license, contributor onerisi

3. **`LICENSE`** — MIT (Directory.Build.props'ta tanimli)

4. **`.editorconfig`** — .NET 10 standart
   - `indent_style = space`, `indent_size = 4`
   - `end_of_line = lf` (CI uyumu)
   - `charset = utf-8-bom` (`.cs` icin `utf-8`)
   - `insert_final_newline = true`
   - `dotnet_sort_system_directives_first = true`
   - C# naming + style kurallari

5. **`.gitattributes`**
   - `* text=auto eol=lf`
   - `*.cs text eol=lf`
   - `*.sln text eol=crlf` (VS uyumu)
   - `*.png binary`, `*.jpg binary`

6. **`.github/workflows/build-test.yml`** — Minimum CI
   - Trigger: `push` + `pull_request` main/develop
   - Steps: checkout → setup-dotnet 10 → `dotnet restore` → `dotnet build --no-restore -c Release` → `dotnet test --no-build -c Release --collect:"XPlat Code Coverage"`
   - Artifact: test results + coverage
   - **Breaking change koruma:** TreatWarningsAsErrors devrede oldugu icin derleme hata varsa fail

7. **`.github/workflows/codeql.yml`** — Opsiyonel, guvenlik analizi
   - GitHub CodeQL default C# template

8. **`.github/workflows/nuget-publish.yml`** — Tag uzerine trigger
   - `on: push: tags: - 'v*'`
   - `dotnet pack` + `nuget push` (NUGET_API_KEY secret)
   - Kullanici onayi olmadan **uretim secret ayarlamaz**, sadece workflow dosyasi eklenir

9. **`CHANGELOG.md`** — Keep a Changelog formati
   - `## [Unreleased]` baslangic

10. **`CONTRIBUTING.md`** — Temel katki kurallari
    - Conventional Commits, branch naming, PR template

11. **`CODEOWNERS`** — `* @omerkck` varsayilan

12. **`.env.example`** — Sample config anahtarlari (sample.WebApi referansli)

13. **`dotnet-tools.json`** — MinVer, reportgenerator, dotnet-format, dotnet-ef versiyonlari

**Dogrulama:**
- `dotnet build` yesil kalmali
- `dotnet test` yesil kalmali
- Git status: yeni dosyalar ekli, bin/obj ignored

**Breaking change:** Yok.

**Commit stratejisi:** Tek commit: `chore(infra): add repo baseline (gitignore, CI, README, LICENSE, editorconfig)`.

---

## FAZ 1 — Kritik Guvenlik ve Kod Duzeltmeleri

**Hedef:** Rapordaki CRITICAL + guvenlik HIGH bulgulari kapat.

**Onay Gate:** `FAZ 1 DEVAM` gerekli.

**Adimlar:**

1. **`JwtTokenService.GetClaimsFromToken` → `internal`** (HIGH, OWASP-A07)
   - Dosya: `src/Providers/Kck.Security.Jwt/JwtTokenService.cs:142-149`
   - Interface `ITokenService`'de bu metot yoksa dogrudan kaldir veya `internal` yap
   - Interface'de varsa: `[Obsolete("Use ValidateTokenAsync. This does not verify signature.")]` + ownership check
   - **Breaking change riski:** Eger interface'de bu metot public ise **major version bump** gerekli

2. **Samples hardcoded "guest" fallback kaldir** (MEDIUM, OWASP-A05)
   - Dosya: `samples/Kck.Sample.WebApi/Program.cs:44-46`
   - `?? "guest"` yerine `?? throw new InvalidOperationException("RabbitMq:User required")`
   - Sample README'ye environment variable listesi ekle

3. **`AddEntityFrameworkCoreInstrumentation()` ekle** (HIGH, Observability)
   - Dosya: `src/Providers/Kck.Observability.OpenTelemetry/DependencyInjection/KckOpenTelemetryBuilder.cs:48-53`
   - `OpenTelemetry.Instrumentation.EntityFrameworkCore` paketini `Directory.Packages.props`'a ekle
   - Tracing builder'a `.AddEntityFrameworkCoreInstrumentation(o => o.SetDbStatementForText = true)` ekle

4. **`CacheServiceBase` semaphore eviction race** (HIGH, Concurrency + Performance)
   - Dosya: `src/Abstractions/Kck.Caching.Abstractions/CacheServiceBase.cs:7`
   - Eviction yerine `GetOrAdd` + `AddRef/Release` reference counting pattern
   - Alternatif: eviction tamamen kaldir (memory cost kabul edilebilirse) — SemaphoreSlim dictionary sonlu sayida key'e baglanir
   - Test: `GetOrSetAsync_ConcurrentCalls_ShouldCallFactoryOnlyOnce` gecmeli + yeni `ShouldNotLoseLockUnderEvictionRace` testi

5. **3 EventBus `OperationCanceledException` dogru handle** (HIGH)
   - `InMemoryEventBus.cs:38, 66`
   - `RabbitMqEventBus.cs:113`
   - `AzureServiceBusEventBus.cs:99`
   - Pattern: `catch (Exception ex) when (ex is not OperationCanceledException)` + OCE yukari propage
   - RabbitMQ: OCE durumunda BasicNack yerine connection disposal
   - Azure: OCE durumunda abandon yerine ct.ThrowIfCancellationRequested

6. **`FluentFtp/PathSanitizer` URL-encoded traversal kontrolu** (LOW ama onemli, OWASP-A01)
   - Dosya: `src/Providers/Kck.FileStorage.FluentFtp/PathSanitizer.cs:5-16`
   - `%2e%2e`, `..%2f`, UTF-8 overlong reddi
   - `Path.GetFullPath` + base root prefix kontrolu

**Dogrulama:**
- `/code-review` + `/adversarial-review` (guvenlik-kritik)
- Tum testler yesil
- `/verification-before-completion`

**Breaking change:** `GetClaimsFromToken` public'ten internal'a gecerse — kullaniciya ayrica sor.

**Commit:** 6 ayri feat/fix commit veya mantikli 2-3 grup halinde.

---

## FAZ 2 — Bagimlilik Riski Azaltma

**Hedef:** Terk edilmis paketleri guvenli alternatifle degistir.

**Onay Gate:** `FAZ 2 DEVAM`. **Her paket degisikligi ayri ADR gerektirir.**

**Adimlar:**

1. **`Konscious.Security.Cryptography.Argon2` degerlendirme** (KIRMIZI — 2021)
   - Alternatif secenekler: aktif fork, `Isopoh.Cryptography.Argon2`, libsodium wrapper
   - Secim kriterleri: OWASP uyumu, .NET 10 native, aktif bakim, CVE gecmisi
   - Karar ADR'i: `docs/adr/0001-argon2-implementation.md`
   - Migration: interface `IPasswordHasher` degismez, ic implementasyon degisir — **backward compatible**

2. **`Hangfire.MySqlStorage` 2.0.3 guncelleme/kaldirma** (KIRMIZI — 2022)
   - Secenek A: Son surum var mi kontrol (`dotnet list package --outdated`)
   - Secenek B: Hangfire.SqlServer veya Hangfire.Redis'e gec (MySQL'i bırak)
   - Secenek C: Tamamen kaldir, sadece InMemory + Quartz
   - Karar ADR: `docs/adr/0002-hangfire-storage.md`

3. **`System.IdentityModel.Tokens.Jwt` → `Microsoft.IdentityModel.JsonWebTokens`** (SARI, modern)
   - `JwtSecurityTokenHandler` → `JsonWebTokenHandler` (~2-3x hizli)
   - API farki minimal, `JwtTokenService` refactoru
   - Test impact: `JwtTokenServiceTests` senaryolari ayni

4. **`Elastic.Clients.Elasticsearch` 8.17.2 uyumluluk testi** (SARI)
   - ES 9.x planinda mı? Dokuman tarama
   - Su anki surum .NET 10 + ES 8.x icin yeterli — karar: simdilik kal, 6 ayda bir gozden gecir

5. **`Dependabot.yml` veya `renovate.json`** — Otomatik bagimlilik guncelleme
   - Dosya: `.github/dependabot.yml`
   - Haftalik schedule, security update daily

**Dogrulama:**
- Tum testler yesil
- Guvenlik paketleri icin `/adversarial-review`
- `dotnet list package --vulnerable` temiz

**Breaking change:** Argon2/JWT degisikligi public API'yi etkilemez; storage degisikligi user-config (StorageType string) etkiler — dokumante et.

**Commit:** `refactor(deps): migrate Argon2`, `chore(deps): update Hangfire`, `feat(jwt): switch to JsonWebTokenHandler`.

---

## FAZ 3 — DI Mimari Iyilestirmeleri

**Hedef:** Kutuphane kullanicilarinin hot reload + test edilebilirlik ihtiyaclarini karsila.

**Onay Gate:** `FAZ 3 DEVAM`. **Bu faz breaking change icerir, major version bump gerekebilir.**

**Adimlar:**

1. **`IOptions<T>` → `IOptionsMonitor<T>` gecisi** (HIGH)
   - Etkilenen: `JwtTokenService`, `Argon2HashingService`, `MailKitOptions`, `RedisCacheOptions`, `RabbitMqOptions`, tum `*Options` ile baglanan Singleton servisler
   - Yaklasim: constructor `IOptionsMonitor<T>`, `CurrentValue` kullan, `OnChange` ile cache invalidation
   - JWT key rotation testi eklenebilir
   - **Breaking change:** Public API'de `IOptions<T>` param alan ctor varsa etkilenir

2. **`EfUnitOfWork` Service Locator kaldir** (HIGH)
   - Dosya: `src/Providers/Kck.Persistence.EntityFramework/UnitOfWork/EfUnitOfWork.cs:15,65`
   - Yaklasim: `IRepositoryFactory<TEntity,TId>` interface + DI kayit
   - `Repository<T,TId>` metodu factory uzerinden
   - Test: mock'lanabilir ve compile-time guvenceli

3. **`Kck.Caching.Redis` `ConnectAsync` + `IHostedService`** (HIGH)
   - `ConnectionMultiplexer.Connect()` blocking → `ConnectAsync()`
   - `IHostedService` ile startup warm-up
   - `IConnectionMultiplexer` DI kaydi `Lazy<Task<ConnectionMultiplexer>>` veya factory
   - Saglik: `ConnectAsync` failure handler + `health/ready` bagla

4. **`AddKckJob<TJob>()` helper** (HIGH)
   - Hangfire: `IServiceCollection AddKckJob<TJob>()` → `AddScoped<TJob>()` + Hangfire metadata
   - Quartz: ayni pattern + `JobDetail` builder

5. **`Kck.Exceptions` → `Kck.Exceptions.Abstractions` ayir** (MEDIUM, Modulerlik)
   - `Kck.Exceptions` provider'indan exception tiplerini Abstractions'a cikar
   - `Kck.Pipeline.MediatR` + `Kck.Exceptions.AspNetCore` sadece Abstractions'a baglanir
   - Provider→Provider bagimliligi kalkar

6. **`TryAddSingleton` tutarliligi** (LOW)
   - Tum provider'larda `AddSingleton` yerine `TryAddSingleton` — kullanici override eklemek istedigi zaman

**Dogrulama:**
- `/adversarial-review` (mimari degisiklik)
- Tum test yesil
- Samples + Bundle ile end-to-end sanity check
- ADR: `docs/adr/0003-options-monitor-adoption.md`

**Breaking change:** EVET, major version. CHANGELOG'a **BREAKING** flag.

---

## FAZ 4 — Hata Yonetimi ve Performans

**Hedef:** Kalan HIGH hata/performans bulgulari.

**Onay Gate:** `FAZ 4 DEVAM`.

**Adimlar:**

1. **`FluentFtpService` connection pool** (HIGH, Performans)
   - `SmtpConnectionPool` deseni uygula (`Channel<AsyncFtpClient>`)
   - `Interlocked` ile aktif sayimi
   - Lifecycle: idle timeout + max pool size

2. **`EfRepository.DeleteRangeAsync` tek call** (MEDIUM)
   - `ExecuteDeleteAsync` (EF 7+) veya `RemoveRange` + tek `SaveChanges`
   - Soft delete durumunda `ExecuteUpdateAsync` + `IsDeleted=true`

3. **`EfRepository` `QueryOptions` Value Object** (MEDIUM, Kod Kalitesi)
   - `bool withDeleted, bool enableTracking, bool permanent` yerine `record QueryOptions(bool IncludeDeleted, bool AsTracking, bool HardDelete)`
   - Default degerler ile overload koru (backward compat)

4. **`Paginate.cs` async path** (MEDIUM)
   - Ctor senkron `Count()` yerine `IPaginate.CreateAsync(queryable, page, size, ct)` factory
   - Eski ctor `[Obsolete]`

5. **`SmtpConnectionPool._disposed` thread-safety** (MEDIUM)
   - `private int _disposed;` + `Interlocked.CompareExchange(ref _disposed, 1, 0) == 0`

6. **`RedisCacheService.RemoveByPrefixAsync` buyuk set LOH** (MEDIUM)
   - `IBatch` veya `IAsyncEnumerable<RedisKey>` ile chunked delete (1000 key/batch)

7. **Outbox pattern (opsiyonel, yeni feature)** (MEDIUM)
   - `Kck.EventBus.Outbox` abstractions
   - `Kck.Persistence.EntityFramework.Outbox` provider
   - Samples'ta ornek
   - **Bu adim yeni provider ekler, ayri karar noktasi.**

**Dogrulama:**
- Performans testleri (BenchmarkDotNet ekle: `benchmarks/` dizini)
- `/code-review` + `/verification-before-completion`

**Breaking change:** Minor (yeni opsiyonel API'ler, eski korunur).

---

## FAZ 5 — Test Kapsami Genisletme

**Hedef:** 9 eksik provider test + coverage olcumu.

**Onay Gate:** `FAZ 5 DEVAM`.

**Adimlar:**

1. **9 eksik provider icin test projesi olustur** (her biri 5-10 test)
   - `Kck.Logging.Serilog.Tests` — DI, enricher, template format
   - `Kck.Security.Secrets.UserSecrets.Tests` — DI, resolve
   - `Kck.Security.Secrets.AzureKeyVault.Tests` — DI + mock ISecretClient
   - `Kck.BackgroundJobs.Quartz.Tests` — scheduler, job adapter
   - `Kck.Documents.ImageSharp.Tests` — resize, format conversion
   - `Kck.Messaging.MailKit.Tests` — SmtpConnectionPool, SendAsync mock
   - `Kck.Messaging.AmazonSes.Tests` — mock `IAmazonSimpleEmailServiceV2`
   - `Kck.Messaging.SendGrid.Tests` — mock `ISendGridClient`
   - `Kck.FeatureFlags.InMemory.Tests` — CRUD + change notifier

2. **Coverlet + ReportGenerator entegrasyonu**
   - `coverlet.collector` tum test projelerine
   - CI: `dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage`
   - ReportGenerator ile HTML rapor
   - CI artifact + PR yorumu (opsiyonel)
   - Coverage eşigi: **%70 hedef** (simdilik bilgi amacli, enforce etmeden)

3. **Flaky test duzeltme — `FakeTimeProvider`**
   - `InMemoryCacheServiceTests.cs:125,162` → `Microsoft.Extensions.Time.Testing.FakeTimeProvider`
   - `JwtTokenServiceTests.cs:111` → ayni
   - Options sinifina `TimeProvider` enjeksiyonu

4. **`HangfireJobScheduler` gercek is mantigi testleri**
   - Enqueue, Schedule, AddOrUpdateRecurring — mock `IBackgroundJobClient`/`IRecurringJobManager`

**Dogrulama:**
- CI'da `dotnet test` ve coverage yukleme
- `/verification-before-completion`

**Breaking change:** Yok.

---

## FAZ 6 — Dokuman ve ADR

**Hedef:** Surdurulebilirlik altyapisi.

**Onay Gate:** `FAZ 6 DEVAM`.

**Adimlar:**

1. **`docs/adr/` dizini + 6 temel ADR**
   - `0001-argon2-implementation.md` (Faz 2'den referans)
   - `0002-hangfire-storage.md`
   - `0003-options-monitor-adoption.md`
   - `0004-ef-core-over-dapper.md`
   - `0005-mediatr-pipeline.md`
   - `0006-rs256-jwt-key-sourcing.md`

2. **`CHANGELOG.md` semantic release** (MinVer ile entegre)
   - Keep a Changelog formati
   - `[Unreleased]` bolumu
   - Git tag-based generation: `changelog.yml` workflow (opsiyonel)

3. **`docs/` README linkleri** — Modul-basinda kisa kullanim ornegi
   - `docs/providers/caching.md`, `docs/providers/eventbus.md`, vb.
   - Bundle kullanim: `docs/bundles/webapi.md`

4. **XML doc eksik public API icin `/// <summary>`** — Abstractions %80 zaten, Providers kismen
   - Public API'de eksik olanlar: focused sweep

5. **Sample README'ler** — Her sample icin kisaca "bu nedir, nasil calistirilir"

**Dogrulama:**
- `/code-review` (dokuman tutarlilik)
- Link dogrulama

**Breaking change:** Yok.

---

## FAZ 7 — Analyzer ve Kod Temizligi

**Hedef:** `WarningsNotAsErrors` listesini daraltma + kod kokusu temizligi.

**Onay Gate:** `FAZ 7 DEVAM`.

**Adimlar:**

1. **`Directory.Build.props` `WarningsNotAsErrors` tek tek temizlik**
   - CA1000, CA1001, CA1304, CA1305, CA1716, CA1848, CA1852, CA1869, CA1873, CA2016
   - Her kural icin ayri commit: ihlal sayisini duzelt, kuraldan cikar
   - CA1848 (LoggerMessage) — performans-kritik log yerlerine uygula

2. **Serilog PII maskeleme Enricher** (MEDIUM, Observability)
   - `KckSerilogBuilder`'a `PiiMaskingEnricher` ekle
   - Pattern: email, credit card, phone — regex-based mask

3. **`AmazonSesEmailProvider` `To/Cc/Bcc` tek foreach** (LOW, Performans)

4. **`LocalizationService` `ImmutableList`/`FrozenList`** (LOW)

5. **`ElasticsearchSearchService` `IDisposable`** (LOW)

6. **`QuartzJobScheduler` `Lazy<IScheduler>`** (LOW)

7. **`DateTime.UtcNow` → `DateTimeOffset.UtcNow`** global sweep (INFO)

8. **Collection expression `[...]` kullanim genislet** (MEDIUM, Modern)

**Dogrulama:**
- `dotnet build -warnaserror` gec
- Tum testler yesil

**Breaking change:** Yok.

---

## Faz Sonrasi

- `tasks/checkpoint.md` tamamlanmis → `tasks/archive/2026-04-20-audit.md`
- Memory guncelle: kullanici onayli kararlar + non-obvious gerekceler
- `CHANGELOG.md`'ye "2026-Q2 Audit Remediation" ozet
- Release: major version bump (Faz 3 breaking'ler nedeniyle) → v2.0.0 GitHub Release

---

**Plan sonu. Faz 1 icin onay bekleniyor: `FAZ 1 DEVAM` yazarsan baslarim.**
