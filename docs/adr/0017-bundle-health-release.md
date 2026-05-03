# ADR-0017: Bundle Stratejisi, Health Check Pattern ve Release Otomasyonu (LS-FAZ-7)

Tarih: 2026-05-03
Durum: Onaylandi

## Baglam

Library Strategy raporu (2026-04-25) §1.1, §6.3, §10.3, §10.4:

- **§1.1 (HIGH):** Tek bundle (`Kck.Bundle.WebApi`) var; WorkerService ve MinimalApi
  senaryoları için canonical setup kılavuzu yok.
- **§6.3 (MEDIUM):** Release sürecinde CHANGELOG manuel özetleniyor; hata riski taşıyor.
- **§10.3 (HIGH):** Provider'lar `IHealthCheck` sunmuyor; production readiness eksik.
- **§10.4 (MEDIUM):** `IConfiguration` convention belgelenmemiş; kullanıcı hangi
  anahtarı nereye yazacağını kenara iterek deneme yanılmayla buluyor.

## Karar

### 1. Yeni Bundle'lar (§1.1)

**`Kck.Bundle.WorkerService`** — arka plan görev odaklı host için opinionated stack:
- Serilog + OpenTelemetry + InMemory EventBus (varsayılan)
- Hangfire **veya** Quartz — `appsettings.json` `Kck.BackgroundJobs.Provider` ile seçilir
- İkisi birden yasaklı; README uyarısıyla tutarlı

**`Kck.Bundle.MinimalApi`** — WebApi'nin hafif sürümü:
- Serilog + InMemory cache + JWT + exception handling + health checks
- MVC, MediatR pipeline, Argon2 kasıtlı olarak çıkarıldı — yalnızca gerçekten
  ihtiyaç duyan projeler bu paketleri elle ekler

### 2. Release Drafter (§6.3)

`release-drafter/release-drafter@v6` GitHub Action seçildi (release-please'e tercih):
- MonoRepo'da per-tag granularite (release-please CHANGELOG.md'yi yeniden yazıyor,
  mevcut elle tutulan CHANGELOG akışımızla çakışıyor)
- MinVer ile çakışmıyor — sadece draft release oluşturur, tag atmaz
- Conventional commit label eşlemesi: `feat` → minor, `fix/perf/security` → patch,
  `breaking` → major

### 3. IConfiguration Schema (§10.4)

JSON Schema draft-07 formatında `schemas/appsettings.kck.schema.json` oluşturuldu.
Kullanıcı `"$schema"` anahtarıyla IDE autocomplete kazanıyor.
`docs/configuration-schema.md`: her provider için alan tablosu + tam örnek.

### 4. Health Check Pattern (§10.3 — kısmi)

**Strateji:** Saf bağımlılık kontrolü — hiçbir ASP.NET Core middleware bağımlılığı yok.
Her check `internal sealed` (sadece DI üzerinden maruz kalır).

| Provider | Check Adı | Kayıt | Kontrol |
|---|---|---|---|
| `Kck.Caching.Redis` | `redis-cache` | Auto (`AddKckCachingRedis`) | `PING` komutu |
| `Kck.Persistence.EntityFramework` | Kullanıcı belirler | Manuel (`AddKckEfCoreCheck<T>`) | `CanConnectAsync()` |
| `Kck.EventBus.RabbitMq` | `rabbitmq-eventbus` | Auto (`UseRabbitMq`) | Geçici bağlantı |

EF Core kayıt deseninin manuel olma nedeni: provider hangi `DbContext`'in
kullanıldığını bilmiyor; kullanıcı kendi context'ini DI'ya kaydediyor.
Otomatik kayıt reflection gerektirirdi — aşırı karmaşık, yanlış tür seçme riski.

**Ertelenen:** InMemory, AzureServiceBus, FluentFtp, MailKit, Elasticsearch health
check'leri — FAZ-8 veya bağımsız PR kapsamında.

## Alternatifler

| Alternatif | Neden Reddedildi |
|---|---|
| `release-please` | CHANGELOG.md'yi otomatik yeniden oluşturuyor — mevcut elle tutulan CHANGELOG'la çakışıyor |
| Tüm provider'lara health check | 12 provider × 80 satır = 960 satır, tek PR'da fazla değişiklik |
| `IHealthChecksBuilder` yerine `IServiceCollection` extension | `AddHealthChecks()` idempotent, iç içe kayıt güvenli |

## Sonuçlar

- **Pozitif:** WorkerService ve MinimalApi için getting-started süresi düşüyor;
  release süreci otomasyona geçiyor; IDE konfigürasyon rehberi production
  hata sürprizlerini azaltıyor.
- **Negatif:** EfCoreHealthCheck kullanıcı elle eklemeli — sıfır friction değil.
- **Teknik borç:** Kalan 9 provider health check FAZ-8 kapsamına aktarıldı.
