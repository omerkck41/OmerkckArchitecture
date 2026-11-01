# Provider Sağlık Analizi + Atıl Kod / Yapı Tutarlılığı — Bulgular ve Plan

- Tarih: 2026-10-31 · Kademe: analiz K2 (kod değişikliği yok); uygulama fazları K2–K3 (F3 paket yüzeyi değiştirir) ·
  Profil: Kurumsal (TIER 3)
- Girdi: bakımcı isteği (2026-10-31) — her Provider'ın sağlığı, atıl kod/dosya, yerel ↔ GitHub tutarlılığı; sonuçlara göre plan.
- Taban: `main` f669d9c = `origin/main` (0/0), çalışma ağacı temiz; Release build 0 hata (5 bilinçli CA1716). Test: Docker kapalıyken
  1223 geçti / 27 başarısız (hepsi Testcontainers; skip-guard yok); Docker açılıp 7 proje gerçek container'larla yeniden koşunca
  **27/27 geçti → toplam 1250/1250** (Redis, Hybrid, TokenBlacklist, RabbitMQ 1 dk 25 s, Persistence/Npgsql 384, Sample.Orders/Postgres 30,
  Elasticsearch 16 — ilk imaj çekimiyle 7 dk 54 s). Kod hatası yok; bulgu yalnız "skip yerine fail" (F4).
- Durum: **kararlar alındı 2026-10-31** (kullanıcı, sohbet: D1–D6 §6; hepsi önerilen seçenek, D3 SESv2 ekleri, D4 `utf-8`,
  D6 F3 5.0'da). **Üst karar (kullanıcı, 2026-10-31, sonra): tüm geliştirmeler tek sürümde** — ara yama/minor yok, bu plan ve kardeş plan
  bitince **tek 5.0.0** + nuget paketi. D1 (4.8.1 yaması) bu kararla düştü; F0 ilk PR olarak yapılır, yayın en sonda.
  Dosyaların kabulü ve commit bekliyor. Kardeş plan: [SixLabors kaldırma](2026-10-31-sixlabors-kaldirma-plani.md).
- Kanıt yöntemi: 38 provider + 17 abstraction + 3 bundle kaynak okuması (662 .cs), test envanteri (46 proje), repo dışı
  **probe** (SQLite/DI ile 4 senaryo), nuget.org nuspec, lisans kapısı yerel koşusu (257 paket), `dotnet format` analyzer taraması,
  sözleşme → implementasyon eşlemesi (betik). Ayrıntılı not defteri: `tasks/checkpoint.md` (yerel).

## 1. Özet (Pareto — önce bunlar)
| # | Bulgu | Etki | Kanıt | Düzeltme |
|---|---|---|---|---|
| K1 | **`EfRepository` okuma yolu soft-delete olmayan entity'de patlıyor.** `Query()` her T'ye `Where(e => !((ISoftDeletable)e).IsDeleted)` uygular; `Entity<T>`/`AuditableEntity<T>` türevlerinde EF çeviremez → `CountAsync`, `GetByIdAsync`, `GetListAsync`, `AnyAsync`, `GetAsync`, `DeleteAsync(id/predicate)`, `RevertSoftDeleteAsync(id)` **hepsi** `InvalidOperationException` ("The LINQ expression … could not be translated"). Yalnız `FullEntity` çalışıyor; sample/şablon FullEntity kullandığı için fark edilmedi. Testler `PlainEntity` ile yalnız silme yollarını kapsıyor. | YÜKSEK (kritik) | probe P4 (SQLite, 4 metot) | `Query`'de filtreyi `typeof(ISoftDeletable).IsAssignableFrom(typeof(T))` ise uygula (statik readonly); 8 okuma metodu için `PlainEntity` testleri |
| K2 | **`CacheServiceBase.GetOrSetAsync<T>` değer tiplerinde factory'yi hiç çağırmıyor**: miss'te `GetAsync` default(T) döner, `existing is not null` int/bool/Guid için hep true → 0/false döner, hiç yazılmaz. InMemory/Redis/Hybrid üçünü de etkiler. | YÜKSEK | probe P1 (`<int>` → 0, 0 çağrı; `<bool>` → false) | `TryGetAsync` (bulundu/değer) soyut üyesi veya sarmalayıcı kayıt; değer tipi testleri (CsCheck property) |
| K3 | **JWT doğrulaması 2+ rolü bozuyor**: `ValidateTokenAsync` rolleri `TokenValidationResult.Claims` sözlüğünden okur; aynı tipte çok claim `List<object>` olur → `Roles = ["System.Collections.Generic.List`1[System.Object]"]`. Ayrıca `TokenValidationResult.Claims` hiç doldurulmaz → `CustomClaims` doğrulamada okunamaz. | YÜKSEK (yetkilendirme) | probe P2a/P2b | `principal.ClaimsIdentity.FindAll(ClaimTypes.Role)`; `Claims` sözlüğünü doldur; 2 rol + custom claim testleri |
| K4 | **RabbitMQ zehirli mesaj sonsuz döngü**: `requeue = GetDeathCount(x-death) < MaxDeliveryCount`; `basic.nack requeue=true` x-death yazmaz (yalnız DLX ölümleri) → sayaç hep 0 → işlenemeyen mesaj kuyruk başında sonsuza dek döner; kuyruk DLX'siz (`arguments: null`) → requeue=false olsa mesaj düşer, "dead-lettered" logu yanıltıcı. | YÜKSEK (güvenilirlik) | **probe P5 (gerçek `rabbitmq:3-management` container, 2026-10-31):** her zaman fırlatan handler + tek mesaj → 8 s'de **12.281** teslimat (~1.500/s sıcak döngü), `MaxDeliveryCount=5` hiç uygulanmadı; kuyruğu DLX argümanıyla yeniden ilan → `406 PRECONDITION_FAILED` (DLX'siz ilan edilmiş). Kod: `RabbitMqEventBus.cs:620`, `703` | DLX + `x-dead-letter-exchange` ile kuyruk ilanı, sayaç için `x-delivery-count` (quorum) veya yerel yeniden deneme + DLQ; entegrasyon testi |
| K5 | **25/32 pakette gereksiz `FrameworkReference Microsoft.AspNetCore.App`** (kodda hiç `Microsoft.AspNetCore.*` yok): nuspec'e `<frameworkReference>` olarak akıyor → worker/console tüketicisi ASP.NET Core paylaşımlı çerçevesine bağlanıyor (`dotnet/runtime` imajında çöker). | YÜKSEK (dağıtım) | nuget.org `kck.caching.inmemory` 4.8.0 nuspec | `Microsoft.Extensions.*` paket referanslarıyla değiştir (Caching.Memory, Hosting.Abstractions, Diagnostics.HealthChecks, Configuration.Abstractions, Options…); solution-wide lock |
| K6 | Soyutlamanın implementasyonsuz/kullanımsız üyeleri (atıl public API): `IEncryptionService`, `IAuthorizationService`, `IPermissionChecker`, `IFeatureChangeNotifier`+`FeatureChangeEvent`, `IFileConverter`, `IFileMetadataReader`, `IHealthCheckRegistrar`, `IRecurringJob`, `FilterSecurityException` (hiç fırlatılmaz; guard `ArgumentException`→400 atar), `PathHelper.IsValidPath/Sanitize` (hiçbir sağlayıcı çağırmaz), `FilterValidator` (yalnız test), `ICachableRequest.CacheGroupKey` (etkisiz), `IMetricsService.CreateHistogram(buckets)`/`CreateCounter(tags)` (etkisiz), `SmtpConnectionPool._createLock` (kullanılmayan alan). | ORTA (yüzey/doküman borcu) | `contracts.py` + `unreferenced_types.py` taraması | §4 D2: uygula veya `[Obsolete]` (KCK ids) + v5'te kaldır |
| K7 | **AzureBlob yol kaçışı — container dışına okuma ve yazma.** Sağlayıcı `alpha` container'ına yapılandırılmışken `ExistsAsync`/`DownloadAsync("../beta/secret.txt")` **beta**'nın blob'unu döndürür; `UploadAsync(DestinationPath="../beta", FileName="pwned.txt")` **beta**'ya yazar. Azure SDK blob adını URI yoluna ekler, .NET `Uri` `..` segmentini sadeleştirir; sağlayıcı yolu hiç doğrulamaz (`PathHelper.IsValidPath` var, çağrılmıyor). Yol kullanıcı girdisinden geliyorsa çok kiracılı kurulumda kiracı sınırı aşılır. | YÜKSEK (güvenlik) | **probe P6** (Azurite `latest` + gerçek Azure.Storage.Blobs 12.30.1): B1 `True`, B2 `'beta-secret'`, B3 beta blobları `[pwned.txt, secret.txt]` | `..`, mutlak yol ve `%`'li yolu reddet (FluentFtp `PathSanitizer` deseni) her metotta; `PathHelper` sağlayıcılarda kullanılsın; **F0'da düzeltilir** (tek sürüm kararıyla yama kapsamı sorusu düştü). **S3 (probe P7, S3Mock 5.2.3): kaçış yok** — AWS SDK `..`'yi sadeleştirmez, anahtar `tenantA/../tenantB/pwned.txt` olarak literal yazıldı, `tenantB` okunmadı/yazılmadı; S3'te yalnız hijyen (literal `..` anahtarları reddet) |

## 2. Provider sağlık kartı
Sağlık: **İyi** = sözleşmeye uygun, test derinliği yeterli · **Orta** = çalışır, en az bir ORTA bulgu veya davranış testi yok ·
**Zayıf** = YÜKSEK bulgu. "Davranış testi 0" = yalnız DI/validator testi var, ana yol hiç çalıştırılmıyor.

| Provider | Test | Sağlık | Başlıca bulgular (öncelik) |
|---|---|---|---|
| Kck.Persistence.EntityFramework | 308 (+3 Docker) | **Zayıf** → K1 düzelince İyi | K1; `AuditInterceptor` yalnız tarihleri doldurur, `CreatedBy/ModifiedBy/DeletedBy` hiç set edilmez (doc "Automatically populated"); `FullEntity.RowVersion` `[Timestamp]` yalnız SQL Server'da güncellenir (belgeli, persistence.md:377); `Specification` tek OrderBy |
| Kck.Security.Jwt | 40 | **Zayıf** → K3 | K3; `ClockSkew=0` sabit; options bir kez okunur (anahtar rotasyonu restart ister) |
| Kck.EventBus.RabbitMq | 8 (2 Docker) | **Zayıf** | K4 (broker'da kanıtlı); kanal kapanırsa `EnsureConnectionAsync` yalnız `_connection.IsOpen` bakar → kalıcı `AlreadyClosedException`; BasicQos yok (sınırsız prefetch); publisher confirms yok; health check her probe'da yeni AMQP bağlantısı; `EventProcessor` handler'ları izole etmez (biri patlar → mesaj nack → başarılılar tekrar koşar); tüketici yolu testsiz |
| Kck.Caching.InMemory | 16 | Orta | K2; `IOptionsMonitor.CurrentValue` ctor'da bir kez (yeniden yükleme etkisiz, tüm sağlayıcılarda yaygın) |
| Kck.Caching.Redis | 16 (5 Docker) | Orta | K2; `IDistributedCache` kendi bağlantısını açar → uygulama başına 2 Redis bağlantısı; `configureRedis` iki kez çalışır; `RemoveByPrefixAsync` çok-anahtar DEL Redis Cluster'da CROSSSLOT (belgelenmemiş); replika endpoint'leri de taranır |
| Kck.Caching.Hybrid | 14 (3 Docker) | Orta | K2; `IConnectionMultiplexer` fabrikası **senkron** `Connect`/`.GetAwaiter().GetResult()` (ADR-0006 async hosted-service deseniyle çelişir); doc "HybridCache stampede koruması" ama `GetOrSetAsync` override edilmemiş |
| Kck.Security.TokenBlacklist.Redis | 10 (3 Docker) | Orta | `Lazy<ConnectionMultiplexer>` (ExecutionAndPublication) ilk Connect hatasını **kalıcı önbellekler** → geçici kesinti restart'a kadar bozar; senkron Connect; DI'daki multiplexer yeniden kullanılmaz; health check yok |
| Kck.Security.Secrets.UserSecrets | 13 | Orta | `GetSecretAsync<string>` düz değerde `JsonException` (probe P3; AzureKeyVault string'i özel ele alır → aynı sözleşmede iki davranış) |
| Kck.Security.Secrets.AzureKeyVault | 9 | Orta | `SecretClient`/`DefaultAzureCredential` sabit kodlu (enjekte edilemez) → **davranış testi 0**; `:` içeren anahtar Key Vault adında geçersiz (belgelenmemiş) |
| Kck.Security.Argon2 | 12 | İyi | `VerifyAsync` bozuk hash metninde `FormatException` fırlatır (false yerine); hash'ten gelen m/t/p sınırsız (DoS yüzeyi) |
| Kck.Security.Totp | 14 | İyi | SHA256 sabit — Google/Microsoft Authenticator SHA1 dışını yok sayar, README "Google Authenticator compatible" iddiası çelişkili; replay/deneme sayacı process-local (belgeli); CodeLength/StepSeconds doğrulanmaz |
| Kck.EventBus.AzureServiceBus | 9 | Orta | Abonelik filtresi yoksa `sub.X`'e gelen Y tipli mesaj X olarak deserialize edilir (`Subject` kontrol edilmez); abonelikler önceden yaratılmalı (belgelenmemiş); validator "Managed Identity için endpoint" der ama kod yalnız connection string ctor'u kullanır; davranış testi 0 |
| Kck.EventBus.InMemory | 28 | İyi | — |
| Kck.BackgroundJobs.Hangfire | 4 | Orta | Statik `BackgroundJob`/`RecurringJob` API (IBackgroundJobClient/IRecurringJobManager yerine → test edilemez); `AddHangfireServer` her zaman (publish-only yok); yalnız kayıt testleri; LGPL-3.0 (belgeli istisna) |
| Kck.BackgroundJobs.Quartz | 13 | İyi | — (4.8.0 crontab dönüşümü testli) |
| Kck.Messaging.MailKit | 12 | Orta | Varsayılanlar uyumsuz: `Port=587` + `UseSsl=true` → `ConnectAsync(host, 587, useSsl:true)` **implicit TLS**; 587 STARTTLS ister → kutudan çıkan ayar el sıkışmada düşer; `UseSsl=false` → `StartTlsWhenAvailable` (fırsatçı, downgrade). Çözüm `SecureSocketOptions`. Pool `_createLock` atıl; bayat bağlantı tespiti yok; `SendAsync` testi 0 |
| Kck.Messaging.AmazonSes | 10 | Orta | `Attachments`, `FromName`, alıcı adları **sessizce düşer** (Simple content); `SendAsync` testi 0 |
| Kck.Messaging.SendGrid | 6 | Orta | Cc/Bcc yalnız ilk personalization'a eklenir (MailKit ile semantik farkı); hata mesajında yanıt gövdesi yok; `SendAsync` testi 0 |
| Kck.FileStorage.FluentFtp | 11 | Orta | `UploadAsync` `FtpStatus.Failed` → yalnız Warning, normal döner (**sessiz başarı**); `Overwrite=false` + var olan dosya → sessiz Skip; `AddTransient` (ADR-0009/0022 dışı); `DownloadAsync` tüm dosyayı belleğe alır; davranış testi 0 (11 test validator/sanitizer) |
| Kck.FileStorage.AwsS3 | 12 | Orta | Yol doğrulaması yok (`..`, mutlak yol; FTP'de katı) — **P7: KeyPrefix kaçışı yok**, `..` anahtara literal yazılır (hijyen); davranış testi 0 (S3Mock/MinIO container mümkün — MinIO imajı artık Docker Hub'da değil, `adobe/s3mock` çalıştı) |
| Kck.FileStorage.AzureBlob | 5 | **Zayıf** | **K7 (Azurite'te kanıtlı):** `..` ile başka container'dan okuma ve başka container'a yazma; ctor'da `CreateIfNotExists()` **senkron ağ çağrısı** (DI çözümlemesinde bloklama/istisna); `blob.core.windows.net` sabit; davranış testi 0 (Azurite ile mümkün — probe bunu gösterdi) |
| Kck.Documents.ClosedXml | 3 | Orta | `ReadAsync` `CellsUsed()` → satır ortasındaki boş hücre sonraki değerleri **sola kaydırır**; tüm hücreler string (imza `object?`); CSV: `\r` kaçışsız, formül enjeksiyonu (`=`,`+`,`-`,`@`) nötrlenmez, BOM yok; sınıf adı "ClosedXml" ama ClosedXML kullanmaz |
| Kck.Documents.ImageSharp | 12 | İyi (kaldırılacak) | `GetDimensionsAsync` sözleşme "tam decode etmeden" der, kod `Image.LoadAsync`; `Quality` 1-100 doğrulanmaz; decoder sınırı yok → kardeş plan |
| Kck.FeatureFlags.AzureAppConfig | 5 | Orta | Azure bayrak `conditions.client_filters` (yüzde/targeting/zaman) **uygulanmaz** → %10 rollout herkese açık (README'de belgeli ama tehlikeli varsayılan); refresh hatası çağırana fırlar, son iyi snapshot'a düşme yok; `GetValueAsync` üst düzey `parameters` arar (Azure şeması `conditions.client_filters[].parameters`) → hep default; davranış testi 0 |
| Kck.FeatureFlags.InMemory | 11 | İyi | — |
| Kck.Search.Elasticsearch | 14 (6 Docker) | Orta | `IndexDocumentAsync`/`BulkIndexAsync` hata → yalnız Warning; `Update/Delete*` yanıt **hiç kontrol edilmez**; `CreateIndexAsync` fırlatır (tutarsız); `SearchAsync` ham `query_string` (Lucene enjeksiyon/DoS yüzeyi) |
| Kck.Exceptions.AspNetCore | 10 | İyi | `CustomException.AdditionalData` yanıta **hiç yazılmaz** (`UnifiedApiErrorResponse`'ta alan yok; `BadRequestException(message, additionalData)`, `CustomArgumentException.ParamName` istemciye ulaşmaz, belge aksini söyler); `ValidationException` attribute 400 / handler 422 |
| Kck.AspNetCore | 63 | İyi | `InputSanitizer.Sanitize("")` fırlatır; CSP `default-src 'self'` Swagger/Scalar UI'ı kırar (dev'de kapatma notu) |
| Kck.Http.Resilience | 22 | İyi | `HttpResponseMessage` dispose edilmiyor (düşük) |
| Kck.Resilience.Polly | **0 (test projesi yok)** | Orta | 56 satır ince cephe (`AddResiliencePipeline` yeniden adlandırma), soyutlama yok → D5: test ekle veya v5'te kaldır |
| Kck.Hosting.Aspire | 3 | İyi | ince (34 satır) |
| Kck.Localization / .Json / .Yaml | 64 / 11 / 11 | İyi | `string.Format` InvariantCulture (hedef kültür değil); `FormatCurrency(code)` düz birleştirme; YAML liste değeri tip adı |
| Kck.Logging.Serilog | 32 | İyi | PII maskeleme yalnız üst düzey özellik adı (yapısal `{@User}` içi maskelenmez); MinimumLevel Information sabit |
| Kck.Observability.OpenTelemetry | 33 | İyi | K6 (buckets/tags etkisiz; "Creates or retrieves" ama her çağrı yeni enstrüman); `OpenTelemetryGauge.Set` value/tags ayrı yazılır; instance kayıt → Meter dispose edilmez |
| Kck.Pipeline.Mediator | 32 | İyi | K6 (`CacheGroupKey`); `CachingBehavior` Kck `ICacheService` yerine `IDistributedCache` ister (`AddKckCachingInMemory` kaydetmez → ayrı `AddDistributedMemoryCache` gerekir, belgelenmemiş); `AuthorizationBehavior` `ICurrentUserProvider` ister, **hiçbir Kck paketi implementasyon vermez** |
| Kck.Bundle.WebApi | dolaylı (Sample.Orders 29) | Orta | JWT yalnız Issuer/Audience okur; `RsaKeyBase64` ayrı `Configure` edilmezse ValidateOnStart düşer (belgelenmemiş); 6 `[Obsolete]` config sınıfı |
| Kck.Bundle.MinimalApi | **0** | Orta | test yok; aynı JWT notu |
| Kck.Bundle.WorkerService | 6 | İyi | — |
| Kck.Persistence.EntityFramework.Analyzers | 10 | İyi | — |

Kesişen: `IOptionsMonitor<T>` enjekte edilip `CurrentValue` ctor'da bir kez okunuyor (yeniden yükleme etkisiz) — 20+ sağlayıcı;
`TryAdd*` tutarlı (ADR-0009); options validator Security/Messaging/FileStorage/Search'te var, Caching'de yok.

## 3. Atıl kod ve yapı tutarlılığı
### 3.1 Atıl / etkisiz kod
- K6 listesi (soyutlama, üye, alan). Ek: `Microsoft.Extensions.Configuration.UserSecrets` PackageVersion hiçbir projede kullanılmıyor
  (UserSecrets sağlayıcısı AspNetCore.App çerçevesine yaslanmış); `ClosedXmlCsvExporter` adı yanıltıcı.
- Deprecated ve v5.0'da kalkacak 16 `[Obsolete]` üye (bundles Defaults sınıfları, `HangfireOptions.ConnectionString`,
  `JwtOptions.KeySource.SecretsManager`, `EfRepository/IQuery` bool bayraklı `Query`, `ResilienceOptions.CircuitBreakerFailureThreshold`,
  `IEmailService`, `IFeatureEvaluator`) — plana uygun, v5 listesi.
- `dotnet format` analyzer taraması (IDE0051/0052, CA1812/CA1823): kullanılmayan private üye **0**; IDE0060 3 (ReadmeSnippets
  stub parametreleri, bilinçli).
- Docker'sız geliştirici: 27 entegrasyon testi **skip değil fail** (skip-guard yok) → yerelde kırmızı, CI'da yeşil.

### 3.2 Yapı / belge tutarlılığı
- Yerel ↔ GitHub: `main` = `origin/main`, temiz; uzak dallar `main`, `gh-pages`, `release/3.x`; yerelde yalnız git-ignored üretimler
  (`_site/`, `api/*.yml`, `build.log`, `restore.log`, `artifacts/`, `tasks/`, `.claude/`, `sixlabors.lic`). Solution 106 proje; diskteki
  tüm csproj sln'de (şablon içeriği bilinçli dışarıda). Eski worktree artığı yok.
- `PublicAPI.Unshipped.txt` boş değil (ADR-0013 süreci): `Kck.Core.Abstractions` (IDomainEventHandler, 4.6.0'da yayınlandı),
  `Kck.Http.Abstractions` (3 üye, 4.7.0), `Kck.Persistence.Abstractions` (`IQuery.Query(QueryOptions)`, **2026-05**'ten beri) →
  yayınlarda Shipped'e taşıma adımı atlanıyor. ADR-0013 "16 abstraction" diyor, bugün 17 abstraction izleniyor; ayrıca
  `Kck.FeatureFlags.AzureAppConfig`, `Kck.FileStorage.AwsS3`, `Kck.FileStorage.AzureBlob` içinde `PublicAPI.Shipped/Unshipped.txt`
  var ama `PublicApiAnalyzers` yalnız `src/Abstractions/Directory.Build.props`'ta → bu 3 çift dosya **yetim** (hiç okunmuyor; F5: sil
  veya analyzer'ı provider'lara da uygula).
- Kök `README.md` sağlayıcı tablosu eksik: File Storage satırında `AwsS3`, `AzureBlob`; Feature Flags satırında `AzureAppConfig`.
- `.editorconfig` `[*.cs] charset = utf-8-bom`: 486/662 dosya BOM'suz → kural fiilen uygulanmıyor (CI format kontrolü yok).
- `docs/plans/`: dört plan da tamamlanmış (`2026-10-14` "TAMAMLANDI" ve var olmayan `tasks/archive/…` yolunu gösteriyor;
  `docs/archive/2026-10-15-…` farklı bir checkpoint dosyası); v4 planı ve DX planı bitmiş → arşivleme kuralı uygulanmamış.
- `tests/Kck.Benchmarks` sln'de, CI benchmark iş akışı var — tutarlı. `stryker-*.json` 16 config ↔ mutation matrisi birebir.
- Lisans kapısı yerel koşusu: 257 paket, 0 ihlal; istisnalar yalnız Hangfire ailesi (LGPL-3.0), `dotnet-sonarscanner` (LGPL-3.0, CI aracı),
  SixLabors (Split 1.0). "Ücretli lisans" kriterine takılan **tek** bağımlılık SixLabors → kardeş plan.

## 4. Fazlar ve hamleler
Sıra Pareto'ya göre; her faz tek PR, her PR `/code-review` (+ F0/F1/F3 `/adversarial-review`) ve CHANGELOG `[Unreleased]`.
**Tek sürüm:** hiçbir faz tek başına yayınlanmaz; tüm fazlar + kardeş planın S1/S4 hamleleri `main`'de birikir, en sonda **v5.0.0** tag'i
(kullanıcı) + nuget yayını. Paket doğrulama tabanı 4.8.0 kırıcı değişiklikleri `CompatibilitySuppressions.xml` ile belgeler (v4'teki gibi).

| Faz | Hamle | Dosyalar (etki) | Kabul örneği | Kademe | Risk |
|---|---|---|---|---|---|
| **F0 — kanıtlı hatalar (ilk PR)** | K1 `EfRepository.Query` soft-delete filtresi yalnız `ISoftDeletable` T'lerde; K2 `CacheServiceBase` bulundu/değer ayrımı; K3 Jwt rol/claim okuma; UserSecrets `GetSecretAsync<string>` düz değer; **K7** AzureBlob ve AwsS3 yol doğrulaması (`..`, mutlak yol, `%`; FluentFtp `PathSanitizer` deseni paylaşılır) | `EfRepository.cs`, `CacheServiceBase.cs` (+3 sağlayıcı), `JwtTokenService.cs`, `ConfigurationSecretsManager.cs`, `AzureBlobStorageService.cs`, `AwsS3StorageService.cs`, `PathHelper.cs`; testler: `EfRepositoryTests` (PlainEntity ×8 metot), `Caching.*.Tests` (int/bool/Guid), `Jwt.Tests` (2 rol, custom claim), `UserSecrets.Tests`, FileStorage yol testleri (Azurite ile K7 yeniden üretimi) | Ö1–Ö4, Ö17 (§5) | K2 | Düşük — davranış sözleşmeye yaklaşır; `CacheServiceBase` public abstract → yeni abstract üye kırıcı ⇒ 5.0 majörde serbest, yine de `virtual` + varsayılan uygulama tercih |
| **F1 — güvenilirlik** | K4 RabbitMQ (DLX+DLQ ilanı, `x-delivery-count`/yerel sayaç, kanal yeniden açma, `BasicQos`, health check mevcut bağlantıyı kullanır); TokenBlacklist (hosted-service/`IConnectionMultiplexer` paylaşımı, Lazy kaldır); Hybrid async bağlantı; MailKit `SecureSocketOptions`; FluentFtp `Failed` → fırlat, Skip → `bool`/istisna; Elasticsearch yazma hatası fırlat; AzureBlob `CreateIfNotExists` lazy-async; SES ekleri SESv2 `Simple.Attachments` ile gönderilir (önce `/currency-check`: AWSSDK.SimpleEmailV2 4.0.106'da alan doğrulanır, yoksa `NotSupportedException`); AzureAppConfig son-iyi snapshot + refresh hatası log | ilgili sağlayıcılar + README.nuget.md'ler + `docs/providers/*.md` | Ö5–Ö9 | K2 (dış servise yazan davranış değişmiyor, yalnız hata yolu) | Orta — RabbitMQ kuyruk argümanı değişimi var olan kuyrukta `PRECONDITION_FAILED` ⇒ yeni kuyruk adı/argüman opsiyonu + geçiş notu |
| **F2 — sözleşme hijyeni** | `AdditionalData` → ProblemDetails `extensions`; `CacheGroupKey` uygula (grup anahtar seti) veya `[Obsolete]`; metrics `buckets`/`tags` uygula (OTel View) veya `[Obsolete]`; `AuditInterceptor` için opsiyonel `IAuditUserProvider` (CreatedBy/ModifiedBy/DeletedBy); ClosedXml `ReadAsync` `Cells(1, LastCellUsed)`; `FilterSecurityException` guard'da fırlatılsın (403) **veya** `[Obsolete]`; `IRecurringJob` keşfi veya kaldırma; K6 ölü soyutlamalar **5.0.0'da doğrudan kaldırılır** (tek sürüm: ara `[Obsolete]` sürümü yok; hiçbir Kck sağlayıcısı kullanmıyor; `docs/migrations/v4-to-v5.md` listeler) | abstractions + ilgili sağlayıcılar; `docs/policies/deprecation.md` notu; `CompatibilitySuppressions.xml` | Ö10–Ö12 | **K3** (public API kaldırma) | Orta — bu sözleşmeleri kendi kodunda uygulayan tüketici derleme hatası alır ⇒ geçiş rehberi |
| **F3 — paketleme (5.0.0, D6)** | K5: 25 projede `FrameworkReference` → `Microsoft.Extensions.*` paketleri; `Directory.Packages.props` yeni sürüm satırları; solution-wide `restore --force-evaluate` + `--locked-mode`; `nuspec` doğrulaması | 25 csproj + lock dosyaları + CPM | Ö13 | **K3** (paket bağımlılık yüzeyi, ADR) | Orta — tüketici bağımlılık grafiği değişir (çerçeve → paketler); API değişmez (package validation geçer) |
| **F4 — test derinliği** | Docker yoksa **skip** (`DockerFact` attribute: `Skip` mesajı); Messaging davranış testleri (MailKit: yerel SMTP sahte sunucu; SendGrid/SES: client enjeksiyonu/`HttpMessageHandler`); FileStorage (MinIO + Azurite Testcontainers); AzureKeyVault/ASB/AppConfig client fabrikası enjeksiyonu + sahte; RabbitMQ tüketici yolu (nack/DLQ) entegrasyon; `Kck.Resilience.Polly.Tests` + README "Microsoft.Extensions.Resilience üzerine ince katman" notu (D5), `Kck.Bundle.MinimalApi.Tests` | tests/* ; `tests/_Shared` | Ö14–Ö15 | K2 | Düşük |
| **F5 — temizlik** | README sağlayıcı tablosu; `PublicAPI.Unshipped` → `Shipped` (release adımına ekle, `dotnet format`/betik); 3 provider'daki yetim `PublicAPI.*.txt` (sil veya analyzer uygula); `.editorconfig` `charset = utf-8` + 176 BOM'lu dosyadan BOM kaldırma (D4; isteğe bağlı CI `dotnet format whitespace --verify-no-changes`); tamamlanmış planları `docs/archive/`'a taşı; `Microsoft.Extensions.Configuration.UserSecrets` PackageVersion sil; `_createLock` sil; `ClosedXmlCsvExporter` → `CsvExporter` (v5, kırıcı) | README.md, PublicAPI dosyaları, .editorconfig, docs/plans, CPM | Ö16 | K1/K2 | Düşük |

Sıradaki 3 hamle (onaydan sonra): (1) F0 PR — K1+K2+K3+K7+UserSecrets, önce RED testler; (2) F1 PR'ı RabbitMQ'dan başlar (en yüksek
güvenilirlik riski); (3) kardeş plan S1 (SkiaSharp sağlayıcısı). Yayın yalnız en sonda: tüm fazlar + S4 bitince **tek v5.0.0** tag'i
(kullanıcı atar/onaylar) → nuget ortam onayı (kullanıcı) → 58 paket doğrulaması → taban 5.0.0 PR'ı.

## 5. Kabul örnekleri (Kabul Kapısı adım 1 — testler bunlardan yazılır)
- **Ö1 (K1):** `class Note : Entity<Guid>`; repo `AddAsync` + `SaveChanges`; `await repo.CountAsync()` → `1`; `GetByIdAsync(id)` →
  entity; `GetListAsync()` → `Count 1`; `AnyAsync()` → true; `DeleteAsync(id)` → siler. Bugün: dördü de `InvalidOperationException`.
- **Ö2 (K2):** boş cache; `GetOrSetAsync<int>("k", () => Task.FromResult(42))` → `42`, factory 1 kez; ikinci çağrı → `42`, factory 0;
  aynı `<bool>` (true) ve `<Guid>`. Bugün: `0`/`false`, factory 0.
- **Ö3 (K3):** `Roles=["admin","editor"]`, `CustomClaims={"tenant":"t1"}` ile token; `ValidateTokenAsync` → `Roles` tam olarak
  `["admin","editor"]`, `Claims["tenant"]=="t1"`. Bugün: `["System.Collections.Generic.List`1[System.Object]"]`, `Claims` boş.
- **Ö4:** `IConfiguration["Api:Key"]="plain"`; `GetSecretAsync<string>("Api:Key")` → `"plain"`. Bugün: `JsonException`.
- **Ö5 (K4):** handler her zaman fırlatan tüketici; 1 mesaj publish → en fazla `MaxDeliveryCount` (5) teslimat, sonra DLQ'da 1 mesaj,
  ana kuyruk boş, log "dead-lettered" 1 kez. Bugün: sınırsız redelivery.
- **Ö6:** RabbitMQ kanal broker tarafından kapatıldı (örn. yanlış exchange tipi hatası) → sonraki `PublishAsync` yeni kanal açıp başarır.
- **Ö7 (MailKit):** varsayılan options (587) ile STARTTLS sunucusuna bağlanır; `UseSsl` yerine `SecureSocketOptions.StartTls` varsayılan.
- **Ö8 (FluentFtp):** sunucu `UploadStream` için `Failed` dönerse `UploadAsync` istisna fırlatır; `Overwrite=false` + var olan dosya →
  belgeli davranış (istisna veya `false`), sessiz değil.
- **Ö9 (Elasticsearch):** geçersiz indeks adıyla `IndexDocumentAsync` → istisna (Warning değil).
- **Ö10:** `new BadRequestException("x", new { Field = "Email" })` → ProblemDetails gövdesinde `"field":"Email"` (extensions).
- **Ö11:** `AuditInterceptor` + kayıtlı `IAuditUserProvider` ("alice") → Added entity `CreatedBy=="alice"`, Modified → `ModifiedBy`.
- **Ö12:** Excel satırı `[A, <boş>, C]` → `ReadAsync` → `["A", null, "C"]` (bugün `["A","C"]`).
- **Ö13 (K5):** `dotnet pack Kck.Caching.InMemory` → nuspec'te `frameworkReferences` yok; `Microsoft.Extensions.Caching.Memory` bağımlılığı
  var; `Microsoft.NET.Sdk.Worker` tüketicisi `dotnet run` ile çalışır (AspNetCore runtime'sız imajda).
- **Ö14:** Docker kapalıyken `dotnet test` → 0 başarısız, 27 **atlandı** ("Docker daemon not reachable").
- **Ö15:** MailKit sahte SMTP sunucusuna `SendAsync` → MIME'da To/Cc/Bcc ve ek doğru; SES `Attachments` dolu → sahte client'a giden
  `SendEmailRequest.Content.Simple.Attachments` dosya adı, MIME tipi ve içeriği korur, `FromName` "Ad <adres>" biçiminde (D3).
- **Ö16:** README tablosunda `Kck.FileStorage.AwsS3`, `Kck.FileStorage.AzureBlob`, `Kck.FeatureFlags.AzureAppConfig`; 3 `PublicAPI.Unshipped.txt`
  yalnız `#nullable enable`.
- **Ö17 (K7):** Azurite'te container `alpha`'ya ayarlı sağlayıcıda `ExistsAsync("../beta/secret.txt")` → `ArgumentException` (yol reddi),
  `UploadAsync(DestinationPath="../beta")` → `ArgumentException`, beta değişmez; `ok.txt` normal akış çalışır. S3'te aynı yollar reddedilir
  (bugün literal `tenantA/../tenantB/...` anahtarı yazılıyor).

## 6. Kullanıcı kararları (onay paketi — role.md §2.2)
| # | Karar | Seçenekler | Öneri · **Karar (kullanıcı, 2026-10-31)** |
|---|---|---|---|
| D1 | F0 yayın biçimi | (a) 4.8.1 yama şimdi · (b) 4.9.0 ile birlikte | (a) · ~~Karar: (a)~~ → **üst karar (kullanıcı, sonra): ara sürüm yok, her şey tek 5.0.0'da**; F0 ilk PR, yayın en sonda. Sonuç: K1–K3/K7 düzeltmeleri 5.0.0'a kadar nuget'te yok (bilinçli) |
| D2 | K6 ölü soyutlamalar ve `FilterSecurityException` | (a) `[Obsolete]` + v5'te kaldır · (b) uygula (IAuthorizationService, IFileConverter…) · (c) `FilterSecurityException`'ı guard'da fırlat (400→403) | (a); `FilterSecurityException` için (a) — 403 "kaynak var ama yetkin yok" anlamı dinamik filtre girdisi için yanlış · **Karar: (a)**; tek sürümle ara `[Obsolete]` adımı yok → **5.0.0'da doğrudan kaldırma** + geçiş rehberi |
| D3 | SES ekleri | (a) `NotSupportedException` + belge · (b) Raw MIME (MimeKit bağımlılığı) · (c) SESv2 `Simple.Attachments` (2025'te eklenen yerel ek desteği, ek bağımlılık yok; SDK alanı `/currency-check` ile doğrulanır) | (c) · **Karar: (c)**; alan SDK'da yoksa (a)'ya düşülür |
| D4 | `.editorconfig` `utf-8-bom` | (a) kuralı `utf-8` yap + 176 BOM'lu dosyadan BOM kaldır · (b) `utf-8-bom` koru, 486 dosyaya BOM ekle · (c) kuralı sil | (a) · **Karar: (a)** |
| D5 | `Kck.Resilience.Polly` | (a) test projesi + belge ("façade") · (b) v5'te kaldır | (a) (AotSmoke kullanıyor) · **Karar: (a)** |
| D6 | F3 zamanlaması | (a) 4.9.0 · (b) 5.0 | (a) teknik olarak mümkün (bağımlılık değişimi minor'da izinli) · **Karar: (b) 5.0** — ImageSharp kaldırma ve 16 Obsolete üyeyle aynı major; `/adr` ile kayıt |

~~Yayın treni: 4.8.1 → 4.9.0 → 5.0.0~~ → **Üst karar (kullanıcı, 2026-10-31 sonra): tek sürüm.** Bütün fazlar (F0–F5) ve kardeş planın
S1/S4 hamleleri `main`'de `[Unreleased]` altında birikir; hepsi bitince **v5.0.0** tek tag + nuget yayını (58 paket; `Kck.Documents.ImageSharp`
listeden çıkar, `Kck.Documents.SkiaSharp` eklenir → yine 58). Ara yama, minor veya ön sürüm yok. Bedeli: kanıtlı hatalar 5.0.0'a kadar
yayındaki 4.8.0'da kalır; kazancı: tek geçiş rehberi, tek kırıcı değişiklik dalgası, tek nuget onayı.

## 7. Tehdit modeli · bitiş tanımı · bütçe
- **Kapsam içi:** (1) K1/K2/K3 tüketicilerde yanlış veri/yetki (K3: çok rollü kullanıcı rol kaybı → yetki reddi; "rol yükseltme" yok,
  dize kıyaslaması tutmaz); (2) K4 kuyruk tıkanması (DoS — ölçüldü: tek mesaj ~1.500 teslimat/s, tüketici CPU'sunu ve broker'ı meşgul eder,
  kuyruğun gerisi işlenmez); (3) K5 dağıtım hatası; (4) **K7 Blob yol kaçışı — kanıtlı**: aynı hesap kimliğiyle başka container'a okuma/yazma (kiracı sınırı); S3 KeyPrefix kaçışı P7'de **yok** (literal anahtar);
  (5) `query_string` enjeksiyonu (Elasticsearch — F1'de `simple_query_string` seçeneği).
- **Kapsam dışı:** çok kiracılılık, outbox deseni (belgeli), Hangfire lisans modeli değişimi, yeni sağlayıcı ekleme.
- **Bitiş tanımı:** Ö1–Ö17 CI'da geçer; kapsam içinde açık YÜKSEK bulgu yok; her faz CHANGELOG `[5.0.0]`'da; kardeş plan S4 bitmiş;
  **v5.0.0 nuget'te doğrulanmış** (58 paket, eski sürümler unlist).
- **Tur bütçesi:** faz başına inceleme en fazla 2 tur; F0 ≤ 1 gün, F1 ≤ 3 gün, F2 ≤ 2 gün, F3 ≤ 1 gün (+ADR), F4 ≤ 3 gün, F5 ≤ 0,5 gün.

## 8. Etki haritası (doğrulamayı daraltır)
- Değişen: §4 dosyaları. Referans verenler: `samples/Kck.Sample.Orders` (repository, cache, JWT), `src/Kck.Templates/content`
  (persistence, JWT), `samples/Kck.ReadmeSnippets` (derlenen README blokları — API imzası değişirse CI düşer).
- Tutarlı kalacak kurallar: ADR-0009 (TryAdd), ADR-0010/deprecation.md (Obsolete + DiagnosticId), ADR-0013 (PublicAPI), ADR-0022
  (lifetime), ADR-0028/0029, `docs/licensing.md`, `.github/license-policy.json`, Kabul Kapısı.
- Koruyan testler: ilgili `tests/Kck.*.Tests` + `Kck.Sample.Orders.Tests` (Postgres) + `readme_snippets.py` + package validation (CP0002).

## 9. Riskler
| Risk | Olasılık | Önlem |
|---|---|---|
| K2 düzeltmesi `CacheServiceBase` türetenleri kırar (public abstract) | Orta | yeni üye `virtual` + varsayılan uygulama; CP0002 yoksa minor |
| K4 kuyruk argümanı değişimi mevcut kuyruklarda `PRECONDITION_FAILED` | Yüksek | varsayılan davranışı koru, DLX'i opsiyonla aç; geçiş notu; yeni kuyruklarda varsayılan |
| F3 sonrası tüketici restore'da sürüm çakışması (Microsoft.Extensions.* 10.0.x) | Düşük | mevcut CPM sürümleri zaten 10.0.14; şablon dumanı testi |
| MailKit varsayılan değişimi mevcut yapılandırmaları kırar | Orta | `UseSsl` okunmaya devam eder (true → SslOnConnect), yeni `SecureSocketOptions` null ise porta göre varsayılan (465 → SslOnConnect, 587 → StartTls) (uygulama 2026-11-01: `UseSsl=true` 465 dışı her portta zorunlu StartTls — güvenlik incelemesi kararı, fırsatçı mod yalnız `UseSsl=false`) |
| Tek sürüm: kanıtlı hatalar (K1–K3, K7) 5.0.0'a kadar yayında kalır; iş uzarsa pencere büyür | Orta | F0 ilk PR olarak hemen `main`'e girer (kaynakta düzeltme hazır); sürüm takvimi §7 bütçe; gerekirse kullanıcı kararıyla erken tag |
| Tek sürümde kırıcı değişiklik dalgası büyük (ImageSharp kaldırma, ölü API, paketleme, Obsolete temizliği) | Orta | tek geçiş rehberi `docs/migrations/v4-to-v5.md`, `CompatibilitySuppressions.xml` ile her kırılma belgeli, sürüm notu bölümlenmiş |

## 10. Sınırlar (bu planda düzeltilmez, kayda alındı)
- `IOptionsMonitor.CurrentValue` tek okuma deseni (20+ sağlayıcı) — yeniden yükleme desteği ayrı tasarım kararı.
- Totp SHA256 ↔ Google Authenticator uyumluluğu — algoritma seçeneği (SHA1 varsayılanı kırıcı) v5 adayı.
- `AzureAppConfig` filtre semantiği (targeting/percentage) — ayrı özellik işi; belgeli sınır.
- Localization kültür-bazlı `string.Format` — davranış değişikliği, ayrı karar.
- SixLabors kaldırma → kardeş plan.

## 11. Kaynaklar
- Probe çıktıları (repo dışı, 2026-10-31): `tasks/checkpoint.md` "Probe sonuçları": P1 cache değer tipi, P2 JWT roller, P3 UserSecrets,
  P4 EF SQLite okuma yolu, P5 RabbitMQ zehirli mesaj (Testcontainers `rabbitmq:3-management`, Kck sağlayıcısı + fırlatan handler),
  P6 AzureBlob yol kaçışı (Azurite `latest`, `--skipApiVersionCheck`), P7 S3 KeyPrefix (`adobe/s3mock:5.2.3`; kaçış yok).
- nuget.org nuspec: https://api.nuget.org/v3-flatcontainer/kck.caching.inmemory/4.8.0/kck.caching.inmemory.nuspec (`frameworkReferences`).
- Lisans kapısı raporu: `artifacts/license-report.md` (yerel, 2026-10-31; 257 paket).
- RabbitMQ `x-death` yalnız dead-letter'da eklenir: https://www.rabbitmq.com/docs/dlx ("x-death" header semantics).
