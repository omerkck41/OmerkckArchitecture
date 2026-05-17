# ADR-0022 — DI Lifetime Strategy

**Status:** Accepted  
**Date:** 2026-05-17  
**Deciders:** @omerkck41

---

## Context

Kck framework, 40+ servis kaydeder. Her servis için `Singleton`, `Scoped` veya
`Transient` seçimi doğrudan bellek kullanımı, thread güvenliği ve EF Core uyumunu
etkiler. Şimdiye kadar yazılı bir politika yoktu; geliştiriciler kural yerine
içgüdüyle karar veriyordu.

---

## Decision

Aşağıdaki üç kurala göre lifetime seçilir:

### Kural 1 — Durum taşıyan servisler → Singleton

Servis, paylaşılan durum (bağlantı havuzu, kilit dizisi, dahili cache) tutuyorsa
**Singleton** kullan.

| Servis | Gerekçe |
|---|---|
| `ICacheService` (InMemory / Redis / Hybrid) | 64-stripe lock dizisi, `ConcurrentDictionary` key takibi |
| `IEventBus` (RabbitMq / Azure / InMemory) | Broker bağlantısı, subscription haritası |
| `IEmailProvider` / `IEmailService` | SMTP bağlantı havuzu (`SmtpConnectionPool`) |
| `ISearchService<T>` | Elasticsearch client (bağlantı havuzu) |
| `ISecretsManager` | HTTP client / Key Vault connection |
| `IHashingService` | Durumsuz ama ağır Argon2 parametreleri sabit kalmalı |
| `IFeatureFlagService` | In-memory `Dictionary`, değişmez konfigürasyon |
| `ITracingService` / `IMetricsService` | OTel provider handle'ları |
| `ITokenService` | RS256 anahtar yükü bir kez yapılır |
| `ITokenBlacklistService` | Redis bağlantısı |

### Kural 2 — EF Core bağımlı servisler → Scoped

`DbContext` Scoped'dur. Ona bağımlı her servis de **Scoped** olmalıdır.
Captive dependency (Singleton içinde Scoped) build-time veya runtime hataya yol açar.

| Servis | Gerekçe |
|---|---|
| `IRepository<T, TId>` | `AppDbContext` Scoped; lifetime eşleşmeli |
| `IReadRepository<T, TId>` | Aynı gerekçe |
| `IWriteRepository<T, TId>` | Aynı gerekçe |
| `IUnitOfWork` | `DbContext` wrap'i, Scoped olmalı |
| `AuditInterceptor` | `DbContext` pipeline'ında çalışır |

> **Uyarı:** `IRepository<T, TId>`'yi Singleton servis içinde inject etme.
> `IServiceScopeFactory` kullanarak `using var scope = factory.CreateScope()` yap.

### Kural 3 — Durumsuz, hafif servisler → Singleton (tercih) veya Transient

Durumsuz servisler teknik olarak Transient da olabilir ama her istek için
allocation üretmemek için **Singleton** tercih edilir. Çok nadir istisnalar:

| Servis | Lifetime | Gerekçe |
|---|---|---|
| `ILocalizationService` | Singleton | Kaynak dosyaları startup'ta bir kez yüklenir |
| `IDocumentService` | Singleton | ImageSharp/ClosedXml client durumsuz |
| `IFileStorageService` | Singleton | FTP bağlantı havuzu |
| `IJobScheduler` | Singleton | Hangfire/Quartz scheduler handle |

---

## DI Kaydı Kuralları

1. **`TryAddSingleton` / `TryAddScoped` / `TryAddTransient` kullan** — çift kayıt koruması.
2. **Bundle içinde lifetime karıştırma:** Eğer Singleton bir servis, Scoped bağımlılığa
   ihtiyaç duyuyorsa `IServiceScopeFactory` inject et, manuel scope oluştur.
3. **`IOptions<T>` kullanımı:** `IOptionsMonitor<T>` Singleton servislere inject edilir
   (değişiklik bildirimi destekler, Scoped snapshot vermez). `IOptionsSnapshot<T>`
   yalnızca Scoped servislerde kullanılır. Detay: ADR-0004.

---

## Uygulama Notları

- Tüm `ServiceCollectionExtensions` `TryAdd*` metodlarıyla kayıt yapar (ADR-0009).
- Singleton servislerde thread güvenliği zorunludur; `lock` yerine `SemaphoreSlim`
  veya `ConcurrentDictionary` kullan (CacheServiceBase örneği).
- `IDisposable` Singleton'lar DI container tarafından uygulama kapanışında dispose
  edilir; double-dispose için `GC.SuppressFinalize` ekle (CA1816).

---

## Consequences

**Pozitif:**
- Captive dependency hatası riski minimize edilir.
- Yeni provider ekleyen geliştiriciler hangi lifetime'ı seçeceğini bilir.
- Code review'da lifetime uyumsuzluğu kolayca tespit edilir.

**Negatif:**
- EF Core dışındaki ORM'ler farklı lifetime gerektirebilir; bu ADR o durumda revize edilmeli.

---

## İlgili ADR'lar

- ADR-0004: IOptionsMonitor migrasyonu
- ADR-0005: EF Repository Factory
- ADR-0009: TryAddSingleton tutarlılığı
