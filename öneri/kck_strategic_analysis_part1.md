# Kck Modular Architecture Framework — Stratejik Analiz Raporu (1/2)

**Tarih:** 2026-05-15  
**Kütüphane:** Kck Modular Architecture Framework  
**Tip:** Framework / Middleware  
**Dil:** C# / .NET 10  
**Lisans:** MIT  
**Versiyon:** 2.0.0 (mevcut), Unreleased değişiklikler mevcut  

---

## GENEL VERİMLİLİK SKORU

| Bölüm | Olgunluk | Öncelikli Aksiyon |
|---|---|---|
| Mimari Strateji | 🟢 YEŞİL | Abstractions→Providers→Bundles deseni endüstri standardı; ince ayar yeterli |
| Teknoloji / Framework | 🟡 SARI | net10.0 tek hedef dar kitle riski; Hangfire Newtonsoft.Json transitif CVE |
| Katman Yapısı | 🟢 YEŞİL | 16 abstraction + 35 provider iyi ayrıştırılmış; Pipeline Abstractions eksik |
| Public API Tasarımı | 🟡 SARI | Entity base class çok opinionated; Result<T> implicit operator eksik |
| Performans | 🟡 SARI | CacheServiceBase stripe lock iyi; Entity DateTime.UtcNow allocation tehlikesi |
| Sürdürülebilirlik | 🟢 YEŞİL | SemVer, MinVer, CHANGELOG, ADR, PublicApiAnalyzers — olgun |
| Test Stratejisi | 🟡 SARI | Coverage %40 line / %35 branch düşük; mutation testing yok |
| Dokümantasyon | 🟢 YEŞİL | 17 provider doc, ADR'ler, migration rehberleri — kapsamlı |
| Developer Experience | 🟡 SARI | XML doc 1591 suppress; DebuggerDisplay iyi ama IntelliSense eksik |
| Ecosystem Uyumu | 🟢 YEŞİL | DI extension pattern, Options pattern, ILogger uyumu mükemmel |
| Benchmark / Rekabet | 🟡 SARI | BenchmarkDotNet mevcut ama 3 benchmark az; CI regression yok |
| Breaking Change Risk | 🟡 SARI | Entity base class değişiklikleri yüksek riskli |
| Güvenlik | 🟢 YEŞİL | SECURITY.md, Dependabot, CVE audit, license audit mevcut |

**Severity Dağılımı:** 0 CRITICAL, 4 HIGH, 12 MEDIUM, 8 LOW

### Risk Matrisi (En Yüksek 3 Risk)

| Risk Alanı | Olasılık | Etki | Risk Skoru | Öncelik |
|---|---|---|---|---|
| Test coverage düşük — regresyon riski | Yüksek | Orta | 6 | P1 |
| Entity base class aşırı opinionated — kilitlenme | Orta | Yüksek | 6 | P2 |

---

## BÖLÜM 1 — MİMARİ STRATEJİ

### Mimari Uygunluk

Kck'nin **Abstractions → Providers → Bundles** üç katmanlı mimarisi, modüler framework kütüphaneleri için endüstri standardıyla tam uyumludur.

**Mevcut:** Abstractions kontrat tanımlar, Providers teknoloji implementasyonu sağlar, Bundles opinionated preset sunar.  
**Değerlendirme:** Bu, Serilog'un Sinks/Enrichers, MassTransit'in Transport/Middleware, Microsoft.Extensions.*'ın Abstractions/Implementation ayrımıyla birebir örtüşür. **Doğru tercih.**

### Tasarım Deseni Değerlendirmesi

| Desen | Kullanım | Değerlendirme |
|---|---|---|
| Builder Pattern | `KckPersistenceBuilder`, `KckMediatorPipelineBuilder` | ✅ Fluent API ergonomisi iyi |
| Options Pattern | `IOptionsMonitor<T>` (ADR-0004) | ✅ Hot-reload destekli, doğru tercih |
| Factory Pattern | `IEfRepositoryFactory` (ADR-0005) | ✅ Service locator'dan kurtuluş |
| Template Method | `CacheServiceBase` | ✅ GetOrSetAsync stampede koruması iyi |
| Result Pattern | `Result<T>` + extensions | ✅ Fonksiyonel pipeline (Map/Bind/Tap/Ensure) |

**Öneri 1.1 — Pipeline Abstractions Paketi**

```
Mevcut       : Pipeline marker interface'leri (ICachableRequest, ISecuredRequest vb.) 
               Core.Abstractions içinde yaşıyor
Öneri        : Kck.Pipeline.Abstractions ayır — Mediator/MediatR bağımsız kontrat paketi
Kategori     : Mimari
Olmasa       : Pipeline behavior kontratları Core ile gereksiz bağımlılık oluşturuyor
Maliyet      : Orta (1h)
Beklenen Kazanım: Tüketici sadece pipeline kontratını alıp kendi mediator'ını kullanabilir
Referans     : MassTransit.Abstractions, MediatR.Contracts
Karar        : [ ] Hemen Uygula  [x] 3-Proje Trial  [ ] Reddet
```

**Öneri 1.2 — Decorator/Wrapper Extension Point**

```
Mevcut       : Provider'lar doğrudan ICacheService implementasyonu sağlıyor
Öneri        : ICacheService için dekoratör zinciri desteği (metrics, logging, circuit-breaker)
Kategori     : Mimari
Olmasa       : Kullanıcı cross-cutting concern ekleyemiyor
Maliyet      : Orta (1h)
Beklenen Kazanım: Scrutor benzeri dekoratör ile %0 kod değişikliği ile observability
Referans     : Scrutor, Polly ResiliencePipeline
Karar        : [ ] Hemen Uygula  [x] 3-Proje Trial  [ ] Reddet
```

### Katmanlama Stratejisi

- **Dependency Inversion:** ✅ Tüm provider'lar abstraction'a bağımlı, tersi yok
- **Abstractions ayrı paket:** ✅ 16 ayrı NuGet paketi olarak yayınlanıyor
- **AOT uyumluluk:** ✅ `IsAotCompatible=true` tüm abstraction'larda aktif

---

## BÖLÜM 2 — TEKNOLOJİ VE FRAMEWORK DEĞERLENDİRMESİ

### Hedef Platform

### Bağımlılık Ayak İzi

| Bağımlılık | Risk | Değerlendirme |
|---|---|---|
| Hangfire 1.8.23 → Newtonsoft.Json 11.0.1 transitif | ⚠️ HIGH | CVE GHSA-5crp override 13.0.4 ile yapılmış — ama Hangfire 2.x'e geçiş plansız |
| MediatR 14.1.0 | ℹ️ INFO | Deprecated (KCK0200), Mediator'a geçiş planlanmış — doğru |
| Konscious.Security.Cryptography.Argon2 1.3.1 | ⚠️ MEDIUM | Son commit 2019, bakımsız; .NET 10 Argon2 bcl planı kontrol edilmeli |
| SixLabors.ImageSharp 3.1.12 | ℹ️ INFO | Apache 2.0, aktif bakım, uyumlu |
| ClosedXML 0.105.0 | ℹ️ INFO | MIT, aktif, uyumlu |
| OpenTelemetry EF Core 1.12.0-beta.2 | ⚠️ MEDIUM | Beta paket production bağımlılığı — stabil sürüm beklenebilir |

**Öneri 2.2 — Konscious.Argon2 Alternatifi**

```
Mevcut       : Konscious.Security.Cryptography.Argon2 1.3.1 (son güncelleme 2019)
Öneri        : .NET 10'da System.Security.Cryptography.Argon2 varsa migrate et; 
               yoksa Isopoh.Cryptography.Argon2 (aktif bakımlı) değerlendir
Kategori     : Teknoloji
Olmasa       : Bakımsız bağımlılık güvenlik riski ve .NET 10 uyumluluk problemi
Maliyet      : Orta (1h)
Beklenen Kazanım: Sıfır 3.parti bağımlılık veya aktif bakımlı alternatif
Referans     : ASP.NET Core Identity Argon2id (built-in .NET 9+)
Karar        : [ ] Hemen Uygula  [x] 3-Proje Trial  [ ] Reddet
```

**Öneri 2.3 — OpenTelemetry Beta Paketi**

```
Mevcut       : OpenTelemetry.Instrumentation.EntityFrameworkCore 1.12.0-beta.2
Öneri        : Stabil sürüm çıkınca güncelle; beta'yı CHANGELOG'da belirt
Kategori     : Teknoloji
Olmasa       : API kırılma riski, tüketicilerde restore uyarıları
Maliyet      : Hafif (<1g)
Beklenen Kazanım: Stabil API garantisi
Referans     : OpenTelemetry .NET release policy
Karar        : [x] Hemen Uygula  [ ] 3-Proje Trial  [ ] Reddet
```

---

## BÖLÜM 3 — KATMAN DEĞERLENDİRMESİ

### Gereksiz Katmanlar

**Değerlendirme: Gereksiz katman tespit edilmedi.** 16 abstraction'ın her biri net bir sorumluluğa sahip, provider'lar 1:1 abstraction referansı kullanıyor, bundle'lar sadece birleştirici rol üstleniyor.

### Eksik veya Zayıf Katmanlar

**Öneri 3.1 — Kck.Pipeline.Abstractions Eksik**

```
Katman       : Pipeline Abstractions
Durum        : Eksik
Öneri        : ICachableRequest, ISecuredRequest, ILoggableRequest, ITransactionalRequest
               Core.Abstractions'tan ayır → Kck.Pipeline.Abstractions
Referans     : MediatR.Contracts ayrı NuGet paketi
```

**Öneri 3.2 — Kck.Validation.Abstractions Eksik**

```
Katman       : Validation Abstractions
Durum        : Eksik
Öneri        : FluentValidation'a doğrudan bağımlılık yerine IValidator<T> soyutlaması
Referans     : MassTransit.Abstractions validation kontratları
```

### Daha İyi Alternatifi Olan Katmanlar

**Öneri 3.3 — Bundle.WebApi MediatR→Mediator Geçişi**

```
Katman       : Kck.Bundle.WebApi
Durum        : Daha iyi alternatifi var
Öneri        : Default provider Kck.Pipeline.MediatR → Kck.Pipeline.Mediator (deprecated 
               olan MediatR'ı bundle default'ta bırakmak tutarsız)
Referans     : ADR-0021 ile uyumlu
```

---

## BÖLÜM 4 — PUBLIC API TASARIMI VE KULLANICI DENEYİMİ

### API Ergonomisi

**Öneri 4.1 — Result<T> İmplicit Operator**

```
Mevcut API   : Result<T>.Success(value), Result<T>.Failure(error) — verbose
Sorun        : Her handler dönüşünde factory metot çağrısı tekrarlanıyor
Öneri        : implicit operator T → Result<T> ve Error → Result<T> ekle
Referans     : ErrorOr<T>, OneOf<T> — implicit conversion ile ergonomi
```

**Öneri 4.2 — Entity Base Class Aşırı Opinionated**

```
Mevcut API   : Entity<TId> = IEntity + IAuditable + ISoftDeletable + DomainEvents + 
               RowVersion — tümü zorunlu
Sorun        : Audit ve soft-delete istemeyen tüketici gereksiz property taşıyor; 
               Entity base class değişiklikleri tüm tüketicileri etkiliyor
Öneri        : Katmanlı base class: Entity<TId> (minimal) → AuditableEntity<TId> 
               → FullEntity<TId> (audit+softdelete+events)
Referans     : ABP Entity/AuditedEntity/FullAuditedEntity hiyerarşisi
```

**Öneri 4.3 — DateTime.UtcNow Default Değer Tehlikesi**

```
Mevcut API   : Entity.CreatedDate { get; set; } = DateTime.UtcNow — static default
Sorun        : Test edilemez, saat bağımlılığı enjekte edilemiyor; Entity 
               oluşturulduğunda otomatik set ediliyor ama DB'den okunduğunda 
               overwrite oluyor — yanlış anlaşılmaya açık
Öneri        : Default kaldır, AuditInterceptor'da TimeProvider.System ile set et
Referans     : .NET 8+ TimeProvider abstraction
```

### Genişletilebilirlik

- ✅ Builder pattern ile fluent konfigürasyon
- ✅ `TryAddSingleton` tutarlı kullanım — tüketici override edebilir
- ✅ Extension method pattern (`AddKck*`) — ekosistem konvansiyonu

### SemVer ve API Kontratı

- ✅ PublicApiAnalyzers 16 abstraction'da aktif — 875 shipped symbol
- ✅ Deprecation policy (KCK0001-0999) — DiagnosticId zorunlu
- ✅ `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt` disiplini

---

## BÖLÜM 5 — PERFORMANS VE VERİMLİLİK STRATEJİSİ

### Kritik Performans Fırsatları

**Öneri 5.1 — Paginate<T> Struct Dönüşümü**

```
Alan         : Paginate<T> — sık oluşturulan, kısa ömürlü
Sorun        : class olarak heap allocation; her sayfalama isteğinde GC baskısı
Öneri        : readonly record struct Paginate<T> — stack allocation
Beklenen Kazanım: Hot path'te %30-50 allocation azalma
```

> **Uyarı:** Bu breaking change'dir. `IPaginate<T>` interface boxing oluşturur — interface kaldırılmalı veya kabul edilmeli.

**Öneri 5.2 — Result<T> Struct Dönüşümü**

```
Alan         : Result<T> — her handler dönüşünde oluşturuluyor
Sorun        : Sealed class, heap allocation zorunlu
Öneri        : readonly record struct olarak yeniden tasarla
Beklenen Kazanım: Handler başına 1 allocation eliminasyonu
```

**Öneri 5.3 — CacheServiceBase String Interpolation**

```
Alan         : BuildKey() → $"{Options.KeyPrefix}{key}"
Sorun        : Her cache çağrısında string allocation
Öneri        : string.Create veya stackalloc + string.Concat kullan
Beklenen Kazanım: Cache hot path'te string allocation %100 eliminasyonu
```

### Async Strateji

- ✅ `ValueTask` doğru kullanılmış: `GetAsync<T>`, `ExistsAsync` (hot path)
- ✅ `ConfigureAwait(false)` tutarlı (`CacheServiceBase`)
- ⚠️ `GetOrSetAsync` `Task<T?>` dönüyor ama `ValueTask<T?>` olabilir

### Cold Start

- ✅ Lazy initialization: Redis `IHostedService` ile async bağlantı (ADR-0006)
- ✅ `TryAddSingleton` ile gereksiz tekrarlı kayıt önleniyor

---

## BÖLÜM 6 — SÜRDÜRÜLEBİLİRLİK STRATEJİSİ

### Versiyon Yönetimi

| Alan | Durum | Değerlendirme |
|---|---|---|
| SemVer | ✅ Uygulanıyor | MinVer ile git tag tabanlı |
| Breaking change politikası | ✅ Tanımlı | `docs/policies/versioning.md` |
| Deprecation stratejisi | ✅ Formal | KCK0001-0999, DiagnosticId zorunlu |
| CHANGELOG | ✅ Güncel | Keep a Changelog formatında |
| Release notes | ✅ Otomatik | release-drafter.yml |

### Paket Yayınlama Kalitesi

| Alan | Durum | Değerlendirme |
|---|---|---|
| Metadata | ✅ Eksiksiz | Description, tags, license, URL |
| Symbol package | ✅ Aktif | `.snupkg` formatında |
| Source Link | ✅ Aktif | GitHub Source Link |
| Deterministic build | ✅ Aktif | CI'da `ContinuousIntegrationBuild=true` |
| Lock file | ✅ Aktif | `RestorePackagesWithLockFile=true` |

**Öneri 6.1 — README NuGet'te Görünürlük**

```
Alan         : NuGet paket sayfası
Sorun        : `PackageReadmeFile` property tanımlı değil — NuGet.org'da README görünmüyor
Öneri        : Directory.Build.props'a <PackageReadmeFile>README.md</PackageReadmeFile> ekle
Maliyet      : Hafif (<1g)
Beklenen Kazanım: NuGet sayfasında zengin içerik, indirme oranı artışı
Referans     : Serilog, MassTransit NuGet sayfaları
```

**Öneri 6.2 — Conventional Commits Enforcement**

```
Alan         : Commit mesaj disiplini
Sorun        : CONTRIBUTING.md'de tanımlı ama CI'da enforce edilmiyor
Öneri        : commitlint GitHub Action ekle
Maliyet      : Hafif (<1g)
Beklenen Kazanım: Otomatik CHANGELOG üretimi güvenilirliği
Referans     : commitlint, semantic-release
```

---

## BÖLÜM 7 — TEST STRATEJİSİ

### Mevcut Durum

| Metrik | Değer | Hedef | Değerlendirme |
|---|---|---|---|
| Line coverage | %40.7 | %65 (orta vade) | 🟡 Düşük |
| Branch coverage | %36.3 | %50 (orta vade) | 🟡 Düşük |
| Test framework | xUnit 2.9.3 | Güncel | ✅ |
| Mock framework | NSubstitute 5.3.0 | Güncel | ✅ |
| Assertion | FluentAssertions 8.3.0 | Güncel | ✅ |
| Integration | Testcontainers (PoC) | 5 provider'a yayılacak | 🟡 Kısıtlı |
| Benchmark | BenchmarkDotNet (3 suite) | 10+ suite | 🟡 Az |
| Mutation testing | Yok | Stryker.NET trial | 🔴 Eksik |
| Property-based testing | Yok | FsCheck/CsCheck trial | 🔴 Eksik |

**Öneri 7.1 — Coverage Gate Yükseltme Yol Haritası**

```
Alan         : Test coverage
Sorun        : %40 line / %35 branch — endüstri ortalamasının (%60-80) altında
Öneri        : Kademeli artış: Q3→%50/%45, Q4→%65/%50, 2027-Q1→%75/%60
Maliyet      : Ağır (1ay+) — test yazımı gerektirir
Beklenen Kazanım: Regresyon riski %60 azalma
Referans     : docs/policies/test-coverage.md zaten yol haritası içeriyor
```

**Öneri 7.2 — Testcontainers Yayılımı**

```
Alan         : Integration test kapsamı
Sorun        : Sadece PostgreSQL PoC; Redis, RabbitMQ, Elasticsearch mock
Öneri        : LS-FAZ-4.5 planını hızlandır — 5 provider'a Testcontainers ekle
Maliyet      : Orta (1h)
Beklenen Kazanım: Mock'un kaçırdığı gerçek davranış hataları yakalanır
Referans     : Testcontainers.Redis, Testcontainers.RabbitMq zaten deps'te
```

**Öneri 7.3 — Mutation Testing Başlangıcı**

```
Alan         : Test kalitesi
Sorun        : Coverage yüksek olsa bile testlerin gerçekten hata yakalayıp 
               yakalamadığı bilinmiyor
Öneri        : Stryker.NET ile Result, Paginate, Filter üzerinde trial başlat
Maliyet      : Orta (1h)
Beklenen Kazanım: Test suite güvenilirlik metriki (mutation score >%70)
Referans     : Stryker.NET, docs/test-strategy.md zaten planlamış
```
