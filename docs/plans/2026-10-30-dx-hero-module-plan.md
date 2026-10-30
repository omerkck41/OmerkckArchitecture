# Benimsenme — Geliştirici Deneyimi + Kahraman Modül (Persistence) — Plan

- Tarih: 2026-10-30 · Kademe: K3 (yeni bağımlılıklar: şablon/analyzer araçları; yeni paket yüzeyi: analyzer; ürün
  yönü kararı) · Profil: Kurumsal (TIER 3)
- Üst karar: benimsenme stratejisi seçenek B (DX) + A (kahraman modül) birleşimi; kullanıcı onayı 2026-10-30; yol
  haritası D5 (DX odağı) ile uyumlu ([2026-10-26-dis-inceleme-yol-haritasi.md](2026-10-26-dis-inceleme-yol-haritasi.md))
- Durum: **onaylandı 2026-10-30** (kullanıcı: "Approve, start F0") · F0 yürütülüyor

## 1. Hedef
Kck'yi ilk kez deneyen bir .NET geliştiricisi tek komutla, kurulum gerektirmeyen, testleri ve belgeleriyle çalışan bir
API üretsin; bu API'nin ilk gösterdiği şey `Kck.Persistence.EntityFramework`'ün başkalarında zor bulunan davranışları
olsun (silme kurallarına uyan soft-delete, commit'e bağlı domain event'ler, denetim alanları). Belgeler İngilizce,
derlenen kodla ve tek bir "ilk 5 dakika" yoluyla.

## 2. Kullanıcı kararları (2026-10-30)
| # | Karar | Seçim |
|---|---|---|
| DX1 | Belge, şablon ve örnek dili | İngilizce (tek dil; Türkçe belge sürdürülmez) |
| DX2 | Şablon varsayılan veritabanı | SQLite; parametreyle PostgreSQL / SQL Server |
| DX3 | Uçtan uca örnek alanı | Sipariş yönetimi (müşteri, sipariş, kalem) |
| DX4 | Yayın ritmi | Aşamalı 4.x minor; kırıcı API değişikliği yok, kırıcı fikirler v5 listesine |

## 3. Mevcut durum (2026-10-30 taraması)
| Alan | Bulgu | Sonuç |
|---|---|---|
| Şablon `kck-webapi` | Tek proje, controller tabanlı WeatherForecast; veritabanı, persistence, test projesi, `.http` dosyası yok; CI'da şablon testi yok (yalnız nuget-publish paketliyor) | Vitrin değil; regresyon koruması yok |
| Belgeler | `docs/providers/*.md` (16 dosya, ~1.500 satır), `faq.md`, `migrations/`, `policies/` büyük ölçüde Türkçe; README İngilizce ama Türkçe notlar içeriyor; 58 `README.nuget.md` İngilizce | DX1 için çeviri + bilgi mimarisi gerekli |
| Belge sitesi | docfx 2.81 (modern şablon), GitHub Pages `omerkck41.github.io/OmerkckArchitecture` yayında; "başlangıç" sayfası yok, sayfalar modül listesi | Giriş yolu yok |
| Derlenen örnekler | README'deki 10 C# bloğu CI'da derleniyor (`readme_snippets.py`, H21); `docs/` blokları derlenmiyor | Kapsam genişletilecek |
| Örnekler | `samples/Kck.Sample.WebApi` (EF InMemory, 4 dosya), `MinimalApi`, `WorkerService`, `AotSmoke`, `ReadmeSnippets` | Persistence'ın güçlü yanlarını göstermiyor (InMemory'de transaction yok) |
| Analyzer | Analyzer projesi yok; KCK0101/KCK0200 yalnız `[Obsolete(DiagnosticId=…)]` | Yeni paket yüzeyi (ADR) |
| Domain event dispatcher | Kütüphanede hazır `IDomainEventDispatcher` uygulaması yok; her kullanıcı kendisi yazıyor. `docs/providers/persistence.md:33` var olmayan `MediatorDomainEventDispatcher`'ı kullanıyor (belge–kod kayması; `docs/` derlenmediği için yakalanmadı) | Kahraman modülde ilk sürtünme noktası: G1.0 + G1.3 + G2.6 (belge düzeltmesi) |
| Görünürlük | GitHub: 0 yıldız, konu (topic) yok; nuget toplam ~70,6 bin indirme, büyük kısmı muhtemelen otomatik (2026-10-26) | Ölçüm tabanı F0'da kaydedilir |

## 4. Kabul örnekleri (Kabul Kapısı adım 1 — testler bunlardan yazılır)
- **Ö1 — 60 saniye:** temiz bir makinede (yalnız .NET 10 SDK) `dotnet new install Kck.Templates::4.6.0` →
  `dotnet new kck-webapi -n Shop` → `cd Shop` → `dotnet run --project src/Shop.Api` → `GET /health/ready` →
  `200 {"status":"Healthy",...}`. CI'da NuGet önbelleği sıcakken `dotnet new`'den 200'e kadar ≤ 60 s.
- **Ö2 — sipariş akışı:** `POST /orders {"customerId":"…","lines":[{"sku":"A-1","quantity":2,"unitPrice":10.0}]}` →
  `201 Created` + `Location: /orders/{id}`; log'da `OrderPlaced {id}` **commit'ten sonra** bir kez. Aynı istekte
  handler hata fırlatırsa → `500` ProblemDetails, veritabanında sipariş yok, `OrderPlaced` log'u yok.
  **Karar (yürütme, 2026-10-30):** satır fiyatı katalogdan (`Product`), istekte yalnız `sku` + `quantity`; "hata"
  örneği stok yetersizliği oldu: sipariş kaydı + koşullu stok düşümü tek transaction'da, stok yetmezse rollback →
  `409`, sipariş yok, stok rezervasyonu geri alınır, `OrderPlaced` hiç dağıtılmaz. Uç noktalar `RequireAuthorization()`
  (G1.5 bilgi hamlesi: `dotnet user-jwts` + standart `AddJwtBearer`).
- **Ö3 — soft-delete:** `DELETE /orders/{id}` → `204`; `GET /orders/{id}` → `404`; `GET /orders/{id}?includeDeleted=true`
  → `200`, `isDeleted: true`, `deletedDate` dolu, kalemlerinin hepsi `isDeleted: true`; müşteri silinmez.
- **Ö4 — sayfalama:** 25 sipariş varken `GET /orders?page=1&size=10` → 10 kayıt, `count: 25`, `pages: 3`, `index: 1`.
  **Karar (yürütme):** parametre adları `IPaginate` ile aynı — `GET /orders?index=1&size=10` (0 tabanlı).
- **Ö5 — testler:** üretilen çözümde `dotnet test` → bütün testler geçer (Ö2–Ö4'ü uçtan uca doğrulayanlar dahil).
- **Ö6 — diğer veritabanları:** `dotnet new kck-webapi -n Shop --database postgres` (ve `sqlserver`) → `dotnet build`
  0 uyarı; bağlantı cümlesi `appsettings.json`'da yok, `dotnet user-secrets` komutu README'de.
- **Ö7 — sırsız şablon:** üretilen hiçbir dosyada parola, anahtar, imzalama anahtarı yok; JWT anahtarı user-secrets /
  `dotnet user-jwts` ile; Development dışında ProblemDetails yığın izi göstermez.
- **Ö8 — belgeler:** yayınlanan sitede Türkçe metin yok (CI koruması); `docs/` altındaki her C# bloğu CI'da derlenir;
  docfx derlemesi kırık bağlantıda düşer.
- **Ö9 — analyzer:** Kck.Persistence.EntityFramework'e başvuran projede `transaction.Commit()` / `CommitAsync()`
  (`IDbContextTransaction`) çağrısı → `KCK1001` uyarısı ("Commit through IUnitOfWork so domain events are dispatched")
  ve kod düzeltmesi; senkron `DbContext.SaveChanges()` → `KCK1002` bilgi düzeyi. Şablon, örnekler ve testler bu
  kurallarla 0 uyarı derlenir.

## 5. Tehdit modeli · bitiş tanımı · bütçe
- **Kapsam içi:** (1) şablonun güvensiz varsayılanlarının üretime kopyalanması (sabit anahtar, geniş CORS, ayrıntılı hata
  sayfası, HTTP); (2) belgelerdeki kopyala-yapıştır örneklerinde sır / güvensiz yapılandırma; (3) şablona ve analyzer'a
  giren yeni bağımlılıkların tedarik zinciri (lisans kapısı + SHA sabitleme); (4) analyzer'ın yanlış pozitifle derlemeyi
  kırması (kullanıcı `TreatWarningsAsErrors` açıksa).
- **Kapsam dışı:** üretim dağıtım rehberi, çok kiracılılık, kimlik sağlayıcı entegrasyonu, Aspire AppHost şablonu
  (Sınırlar'a yazılır).
- **Bitiş tanımı:** Ö1–Ö9 CI'da veya gösterimle geçer; kapsam içinde açık KRİTİK/YÜKSEK bulgu yok; her faz sürümü nuget'te
  doğrulanmış.
- **Tur bütçesi:** her PR için inceleme en fazla 2 tur; faz başına süre tahmini §8'de. Dolunca dur → kullanıcıya.

## 6. Alternatifler
| Konu | Yaklaşım | Artı | Eksi | Karar |
|---|---|---|---|---|
| Şablonun yeri | (a) `kck-webapi`'yi yenile · (b) yeni `kck-api` ekle, eskisi kalsın | (a) tek giriş kapısı, bakım tek · (b) eski çıktı korunur | (a) eski çıktıyı bekleyen yok (üretilmiş projeler etkilenmez) · (b) iki şablon, karışık mesaj | **(a)**; eski çıktı `--minimal` ile korunmaz, CHANGELOG'da "template output changed" |
| Uç nokta stili | (a) Minimal API + özellik dilimleri (VSA) · (b) controller | (a) .NET 10 varsayılanı, az tören, dilim = klasör · (b) tanıdık | (b) daha çok kalıp kod | **(a)**; `rules/dotnet.md` VSA ile uyumlu |
| Komut işleme | (a) Mediator + Kck `TransactionBehavior` · (b) endpoint içinde doğrudan `IUnitOfWork` | (a) commit→event sırası tek yerde, vitrinin ana hikâyesi · (b) daha az kavram | (a) bir kavram fazla | ~~(a)~~ → **(b)** — Karar (yürütme): bundle Mediator'a başvuruyor ama kaydetmiyor; şablona Mediator.SourceGenerator + pipeline eklemek kavram ve 60 sn yolunu büyütür; G1.0 ile event'ler Mediator'sız işleniyor. Mediator F2 how-to'da |
| Etkileşimli API arayüzü | (a) Scalar.AspNetCore (Development) · (b) yalnız OpenAPI JSON + `.http` dosyası | (a) "60 sn vay be" anı · (b) bağımlılık yok | (a) yeni bağımlılık | **(a) koşullu** — G1.2 `/currency-check` geçerse; geçmezse (b) (Plan B). `.http` her durumda var |
| Veritabanı şeması | (a) Development'ta `EnsureCreated` · (b) şablonda migration | (a) sağlayıcıdan bağımsız, 60 sn · (b) üretim yolu | (b) her `--database` için ayrı migration, bakım yükü | **(a)** + README'de "switch to migrations" bölümü; `EnsureCreated` yalnız Development'ta (Ö7) |
| Çözüm dosyası | (a) `.slnx` · (b) `.sln` | (a) SDK 10 `dotnet new sln` varsayılanı · (b) eski araçlar | (a) VS 17.13 öncesi açamaz | **(a)**; G1.1'de VS/Rider/VS Code ile açılış doğrulanır, sorun çıkarsa (b) |
| Şablon testi | (a) CI'da `dotnet new` + build + run + HTTP kontrol betiği · (b) Microsoft.TemplateEngine.Authoring.TemplateVerifier anlık görüntü testi | (a) bağımlılık yok, gerçek davranış · (b) çıktı farkını yakalar | (a) dosya farkını görmez · (b) yeni bağımlılık, kırılgan | **(a)**; (b) Sınırlar'a |
| Belge bilgi mimarisi | (a) Diátaxis (tutorials / how-to / reference / explanation) · (b) bugünkü modül listesi + çeviri | (a) öğrenme yolu ile başvuru ayrışır · (b) az iş | (b) giriş yolu yine yok | **(a)** |
| Analyzer paketleme | (a) `Kck.Persistence.EntityFramework` paketinin içinde (`analyzers/dotnet/cs`) · (b) ayrı `Kck.Analyzers` paketi | (a) kuran herkes otomatik alır, sürüm eşleşir · (b) isteğe bağlı | (a) kapatmak için `.editorconfig` · (b) kimse kurmaz | **(a)**; ADR-0031 |
| Türkçe → İngilizce | (a) bölüm bölüm yeniden yazım (çeviri değil, Diátaxis'e göre) · (b) makine çevirisi + düzeltme | (a) kalite · (b) hız | (b) yapı eski kalır | **(a)** |

## 7. Mimari (hedef çıktı)
**Şablon çıktısı** (`dotnet new kck-webapi -n Shop`):
```text
Shop/
  Shop.slnx
  README.md                     # run, test, switch database, migrations, next steps (links to docs site)
  Shop.http                     # every endpoint, ready to send
  src/Shop.Api/
    Program.cs                  # AddKckWebApiDefaults + AddKckPersistence(UseEntityFramework + audit + domain events)
    Data/ShopDbContext.cs       # ApplyGlobalSoftDeleteQueryFilter
    Domain/Customer.cs, Order.cs, OrderLine.cs, OrderPlaced.cs
    Features/Orders/PlaceOrder.cs, GetOrder.cs, ListOrders.cs, CancelOrder.cs   # endpoint + command/query + handler
    Features/Orders/OrderPlacedHandler.cs   # logs after commit
    appsettings.json, appsettings.Development.json   # no secrets
  tests/Shop.Api.Tests/
    OrdersEndpointTests.cs      # WebApplicationFactory + SQLite in-memory connection; Ö2–Ö4
```
Parametreler: `--database sqlite|postgres|sqlserver` (varsayılan sqlite), `--include-tests` (varsayılan true),
`--include-sample` (varsayılan true; false → boş `Features/`, persistence bağlı kalır).

**Belge sitesi (Diátaxis):** `docs/getting-started/` (tutorial: ilk 5 dakika → ilk entity → ilk test),
`docs/how-to/` (veritabanı değiştir, migration'a geç, Redis cache, JWT, outbox sınırları), `docs/reference/`
(bugünkü `providers/*.md`'nin İngilizce hâli), `docs/explanation/` (Abstractions → Providers → Bundles, persistence
tasarımı: soft-delete kuralı, commit'e bağlı event'ler, pooling), `docs/persistence/` (kahraman modül vitrin sayfası +
karşılaştırma). `toc.yml` bu sırayla.

**Analyzer:** `src/Analyzers/Kck.Persistence.EntityFramework.Analyzers/` (netstandard2.0, `IsRoslynComponent`),
paketlenmez; `Kck.Persistence.EntityFramework.csproj` çıktısını `analyzers/dotnet/cs` altına koyar. Kimlikler `KCK1xxx`
(KCK0xxx `[Obsolete]` için ayrılmış kalır).

## 8. Fazlar ve görevler
Her görev: test önce (RED görülür — L-009) → uygula → `/verify` → PR → CI → birleştir. Faz sonunda yayın (DX4).

### F0 — Temel (yayın yok) · ~1 oturum
- **G0.1 ADR-0029** "Adoption: DX-first with Persistence as hero module": DX1–DX4, §6 kararları, başarı ölçütleri (§9).
  Teslim: `docs/adr/0029-dx-first-adoption.md` + `docs/adr/README.md` satırı.
- **G0.2 Ölçüm tabanı:** Kck.Templates ve Kck.Persistence.EntityFramework aylık indirme, toplam indirme, GitHub yıldız /
  fork / issue sayısı, tarihli olarak `docs/policies/adoption-metrics.md`'ye (İngilizce). Kaynak: nuget.org API (GET).
- **G0.3 Mevcut şablon için CI koruması** (davranış değişmeden önce): `.github/workflows/template-smoke.yml` — **58 paketin
  hepsini** `./artifacts/feed`'e paketle (PR'daki `4.x.y-alpha` sürümleri nuget.org'da yok — premortem R1), üretilen
  projenin yanına bu yerel kaynağı + nuget.org'u içeren `nuget.config` yaz, `dotnet new install
  ./artifacts/feed/Kck.Templates.*.nupkg`, `dotnet new kck-webapi -n Smoke`, `dotnet build -warnaserror`, çıktıda
  `KCK_PACKAGE_VERSION` gibi yer tutucu kalmadığını doğrula. Test: iş akışı bugünkü şablonla yeşil; şablona bilerek
  derlenmeyen bir satır eklenmiş dalda kırmızı (RED kanıtı).

### F1 — Şablon vitrini → **4.6.0** · ~3 oturum
- **G1.0 Hazır domain event işleyicileri (kütüphane, eklemeli API) + ADR-0030:**
  `Kck.Core.Abstractions`: `public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
  { Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken); }`.
  `Kck.Persistence.EntityFramework`: `public KckPersistenceBuilder AddDomainEventHandlers(params Assembly[] assemblies)`
  — işleyicileri scoped kaydeder ve `internal sealed class ServiceProviderDomainEventDispatcher : IDomainEventDispatcher`'ı
  `AddDomainEventDispatch` yoluyla bağlar (event tipine göre işleyici türü önbelleklenir). Testler:
  `AddDomainEventHandlers_TwoHandlersForOneEvent_BothRun` (sıra sözleşme değil, belgelenir — R11),
  `AddDomainEventHandlers_HandlerForBaseEventType_NotInvoked` (yalnız tam tip eşleşir, belgelenir),
  `AddDomainEventHandlers_NoHandlerForEvent_DispatchSucceeds`,
  `AddDomainEventHandlers_HandlerThrows_EventsGoBackOnEntity` (mevcut hata semantiği),
  `AddDomainEventHandlers_PooledContext_ResolvesHandlersFromNewScope`,
  `AddDomainEventHandlers_WithCustomDispatcher_Throws` (ikisi birlikte belirsiz → açık hata). PublicAPI.Unshipped + belge.
  Alternatifler (ADR-0030): Mediator köprüsü `Kck.Pipeline.Mediator`'da (bağımlılık yönünü değiştirir), dispatcher'ı
  Abstractions'a taşımak (kırıcı → v5), şablonda kalması (her kullanıcı yine yazar) — elendi.
- **G1.1 Çözüm iskeleti:** §7 dosya ağacı, `.slnx`, `--database`/`--include-tests`/`--include-sample` sembolleri,
  `template.json` `primaryOutputs` + `postActions` (restore). Şablon çıktısı CPM altında değil: üçüncü taraf sürümleri
  (EF sağlayıcıları, Mediator.SourceGenerator, Scalar, test paketleri) paketleme anında `Directory.Packages.props`'tan
  damgalanır — bugünkü `KCK_PACKAGE_VERSION` mekanizmasının genişletilmesi (R5). Test: `template-smoke.yml` üç
  `--database` değeri için `dotnet build -warnaserror` + `--include-tests false` ve `--include-sample false`
  kombinasyonları; damgalanmamış sürüm yer tutucusu → hata.
  Ek (kör noktalar §13): `.template.config/ide.host.json` + simge; smoke matrisine `-n My.Shop` ve `-n my-shop`;
  ubuntu + windows; çıktıda `.gitignore` (SQLite dosyası dahil).
- **G1.2 `/currency-check`:** Scalar.AspNetCore (lisans, sürüm, EOL, registry) → `decisions/<yürütme tarihi>-template-scalar.md`.
  Geçmezse Plan B (yalnız OpenAPI + `.http`).
- **G1.3 Domain + persistence bağlantısı:** `Order : FullEntity<Guid>` (`FullEntity` zaten `ISoftDeletable`; kalemler
  `DeleteBehavior.Cascade`), `Customer`, `OrderPlaced : IDomainEvent`, `OrderPlacedHandler :
  IDomainEventHandler<OrderPlaced>`; `AddKckPersistence(p => p.UseEntityFramework<ShopDbContext>(…).AddAuditInterceptor()
  .AddDomainEventHandlers(typeof(Program).Assembly))`; Development'ta `EnsureCreated`.
  Test (üretilen test projesinde): `PlaceOrder_ValidRequest_Returns201AndDispatchesOrderPlacedAfterCommit`,
  `PlaceOrder_HandlerFails_PersistsNothingAndDispatchesNothing` (Ö2),
  `Program_BundleAndPersistence_StartsAndResolvesUnitOfWork` (bundle + persistence birlikte hiç denenmedi — §13).
- **G1.4 Uç noktalar:** `POST /orders`, `GET /orders/{id}` (`includeDeleted` sorgu parametresi →
  `QueryOptions(IncludeDeleted: true)`), `GET /orders` (`page`, `size` → `IPaginate<OrderSummary>`), `DELETE /orders/{id}`.
  Testler: `CancelOrder_SoftDeletesOrderAndLines_KeepsCustomer` (Ö3), `ListOrders_25Orders_ReturnsSecondPageShape` (Ö4).
- **G1.5 Güvenli varsayılanlar (Ö7):** **ilk adım bilgi hamlesi (R9):** bundle JWT'yi yalnız `Security:Jwt:Issuer` varsa
  kuruyor; `dotnet user-jwts`'in yazdığı `Authentication:Schemes:Bearer` yapılandırmasıyla uyumu denenir. Karar sırası:
  (a) uyumluysa `/orders` grubu `RequireAuthorization()`, `.http` dosyası `dotnet user-jwts create` belirtecini kullanır,
  Ö2 örneği `Authorization: Bearer` ile; (b) değilse bundle'a eklemeli bir `Security:Jwt` ↔ user-jwts köprüsü
  (ayrı görev, PublicAPI); (c) ikisi de olmazsa uç noktalar anonim kalır ve README + uç nokta yorumu "add authorization
  before production" der — adversarial-review'da açıkça değerlendirilir. Health uç noktaları her durumda anonim (Ö1).
  Diğerleri: sır yok; CORS yalnız
  `Cors:AllowedOrigins` yapılandırmasından; ProblemDetails Development dışında ayrıntısız; HTTPS yönlendirme. Test:
  `template-smoke.yml` üretilen ağaçta sır taraması (anahtar/parola kalıpları) → 0 eşleşme;
  `Template_DefaultRun_NoOutboundTelemetry` (OTLP uç noktası yoksa exporter kayıtlı değil); `/currency-check` yok
  (bağımlılık eklenmez). **`/adversarial-review`** (güvenlik varsayılanları).
- **G1.6 60 saniye ölçümü (Ö1):** `template-smoke.yml` sqlite işi aşamaları ayrı ölçer (`dotnet new`, restore, build,
  başlatma → `/health/ready` 200) ve `GITHUB_STEP_SUMMARY`'ye yazar. Kapı yalnız sıcak önbellekli toplam için: ilk iki hafta
  uyarı, sonra ölçülen p90 + %50 pay ile hata (runner gürültüsü — R4); 60 s hedef olarak raporlanır.
- **G1.9 SQLite sınırları (R3):** para alanları `decimal`; SQLite'ta `decimal` üzerinde sıralama/toplama sorguda yapılmaz
  (toplam bellekte), `[Timestamp]` satır sürümünü SQLite üretmez → eşzamanlılık koruması yalnız PostgreSQL/SQL Server'da;
  ikisi de şablon README'sinde ve G2.4 sayfasında. Test: `ListOrders_OrdersByCreatedDate_OnSqlite`; `--database postgres`
  çıktısının testleri `template-smoke.yml`'de GitHub Actions `services: postgres` konteynerine karşı koşar (bağlantı
  cümlesi ortam değişkeninden; şablona Testcontainers bağımlılığı girmez). Test fabrikası rate limiting'i kapatır (R10).
- **G1.7 Şablon README + `.http`:** çalıştırma, test, veritabanı değiştirme, migration'a geçiş, sonraki adımlar (site
  bağlantıları). Test: README'deki komutlar `template-smoke.yml`'de aynen çalıştırılır.
- **G1.8 Sürüm 4.6.0:** CHANGELOG ("Kck.Templates: kck-webapi output redesigned"), sürüm notu, tag (kullanıcı onayı),
  nuget doğrulaması, taban 4.6.0. `/code-review` her PR'da.

### F2 — İngilizce belgeler + vitrin sayfası (site, nuget yayını yok) · ~4 oturum
- **G2.1 Korumalar önce:** (a) `.github/scripts/docs_language.py` — yayınlanan `docs/**` (plans/archive/memory hariç),
  README, CHANGELOG'un yeni girdileri: Türkçe karakter/sözcük sözlüğü → hata; (b) `readme_snippets.py` kapsamı `docs/**`
  C# bloklarına genişler (`<!-- snippet: skip -->` ile açıkça muaf tutulabilir, sayısı raporlanır); (c) docfx
  `--warningsAsErrors` (kırık bağlantı). **Önce ölç (R6, R7):** `docs/` C# blok sayısı ve kaçının parça (fragment)
  olduğu, docfx'in bugünkü uyarı sayısı ve türleri; kapılar bu tabana göre ratchet ile açılır (muaf blok sayısı yalnız
  azalabilir). RED: bugünkü `docs/providers/persistence.md` (a)'da ve (b)'de (var olmayan `MediatorDomainEventDispatcher`)
  düşer.
  Geçiş süresince (a) yalnız İngilizceye çevrilmiş dosyalara uygulanır (izin listesi), F2 sonunda tamamına.
- **G2.2 Bilgi mimarisi:** `toc.yml` + klasörler (§7); eski `docs/providers/*` bağlantıları yönlendirme sayfasıyla korunur.
- **G2.3 Getting started (tutorial):** "Your first Kck API in 5 minutes" (şablon → Ö2 isteği → soft-delete → test),
  "Add your first entity", "Write your first test". Her adım şablon çıktısıyla CI'da doğrulanır (G1.7 betiği).
- **G2.4 Persistence vitrin sayfası:** sorun → Kck'nin çözümü → kod → sınırlar; konular: silme kurallarına uyan
  soft-delete, commit'e bağlı event'ler (en-az-bir-kez, outbox sınırı dürüstçe), denetim, pooling, sayfalama, dinamik
  sorgu izin listesi.
- **G2.5 Karşılaştırma:** EF Core yerleşik (global/named query filters, interceptor'lar), elle yazılan çözüm, ABP
  Framework ve Ardalis.Specification ile özellik tablosu. **Araştırma önce:** her iddia için tarihli kaynak bağlantısı
  (Anayasa #1, #4); karşı tarafın güçlü yanları da yazılır; `decisions/<yürütme tarihi>-persistence-comparison-sources.md`.
- **ADR'ler (ADR-0029 D2):** 0029'dan itibaren İngilizce; 0001–0028 tarihsel kayıt olarak özgün dilinde kalır ve
  G2.1(a) dil korumasının dışında tutulur (yalnız `docs/adr/00[0-2][0-9]-*.md`, 0029+ değil).
- **G2.6 Başvuru çevirisi:** `docs/providers/*` (16), `faq.md`, `migrations/*`, `policies/*`, `configuration-schema.md`,
  `test-strategy.md` İngilizceye; README'deki Türkçe notlar. Her dosya G2.1(a) izin listesine eklenerek biter.
- **G2.7 Paket README'leri:** `Kck.Persistence.EntityFramework`, `Kck.Bundle.WebApi`, `Kck.Templates` README.nuget.md'leri
  "ilk 5 dakika"ya bağlanır (yayına F4 sürümüyle çıkar).

### F3 — Referans uygulama · ~2 oturum
- **G3.1 `samples/Kck.Sample.Orders`:** şablonun genişletilmiş hâli — müşteri yönetimi, sipariş durumu geçişleri,
  `OrderPlaced` → `Kck.EventBus.InMemory` entegrasyon olayı → log'a yazan işleyici (gerçek e-posta sağlayıcısı
  gerektirmez), dinamik sorgu (`IFilterPropertyWhitelist<Order>`), Redis'siz
  Hybrid cache, OpenTelemetry. `Kck.Sample.WebApi` (InMemory) bununla değiştirilir; README bağlantıları güncellenir.
- **G3.2 Testler:** SQLite ile uçtan uca + `Category=Integration` Testcontainers PostgreSQL (mevcut altyapı). Test adları:
  `CancelOrder_WithShippedLines_KeepsShippedLinesPerDeleteBehavior`, `PlaceOrder_DispatcherFails_EventRetriedOnNextSave`.
- **G3.3 Belgeye bağlama:** how-to sayfaları örnekteki gerçek dosyalara bağlanır (kopya kod yerine).

### F4 — Hata mesajları + analyzer → **4.7.0** · ~3 oturum
- **G4.1 ADR-0031** analyzer paketleme (§6) + `/currency-check`: Microsoft.CodeAnalysis.CSharp (analyzer'ın hedeflediği
  en düşük Roslyn sürümü = .NET 10 SDK'nınki), Microsoft.CodeAnalysis.CSharp.Analyzer.Testing (xUnit v2 uyumu doğrulanır;
  `project_xunit_v3_blocked`). Lisans kapısına eklenir.
- **G4.2 KCK1001** (Ö9): `IDbContextTransaction.Commit/CommitAsync` çağrısı → derleme domain event kullanıyorsa
  (bir `IDomainEventHandler<>` uygulaması ya da `AddDomainEventHandlers`/`AddDomainEventDispatch` çağrısı var) **uyarı**,
  kullanmıyorsa raporlanmaz (R12: event kullanmayanlara gürültü yok) + kod düzeltmesi (`IUnitOfWork.CommitAsync`).
  Testler: `DirectCommit_WithDomainEvents_ReportsKCK1001`, `DirectCommit_WithoutDomainEvents_NoDiagnostic`,
  `UnitOfWorkCommit_NoDiagnostic`, `CodeFix_RewritesToUnitOfWork`. Analyzer, .NET 10 SDK'nın Roslyn sürümünü hedefler;
  G4.1'de SDK 10.0.402 derleyicisinde CS8032 (yüklenemedi) olmadığı doğrulanır (R8).
- **G4.3 KCK1002:** senkron `DbContext.SaveChanges()` → bilgi düzeyi (varsayılan `suggestion`). Testler:
  `SyncSaveChanges_ReportsKCK1002`, `SaveChangesAsync_NoDiagnostic`.
- **G4.4 Kesinlik kapısı:** iki kural tüm `samples/`, `tests/`, şablon çıktısı üzerinde çalıştırılır → yanlış pozitif 0;
  değilse kural bilgi düzeyine iner veya çıkarılır (Plan B).
- **G4.5 Çalışma zamanı mesajları:** `AddDbContextFactory` + `UseEntityFramework` birlikteliği bugün EF'in kapsam hatasıyla
  düşüyor → başlangıçta açık mesaj ("use UseEntityFrameworkPool or register IUnitOfWork per scope…") + belge bağlantısı.
  Test: `UseEntityFramework_WithDbContextFactory_ThrowsWithGuidance`. Diğer mesajlar için tarama: persistence
  `throw new` satırları gözden geçirilir, her mesaj "ne oldu + ne yapmalı + belge bağlantısı" biçimine.
- **G4.6a Yayınlanmamış özellik belgesi (R13):** F4 belgeleri (analyzer, yeni mesajlar) sürüm PR'ıyla birlikte
  birleşir; site main'den yayınlandığı için 4.7.0 nuget'e çıkmadan "Available from 4.7.0" notu olmayan sayfa yayınlanmaz.
- **G4.6 Her diagnostiğin belge sayfası:** `docs/reference/diagnostics/KCK1001.md` … (`helpLinkUri` buraya).
- **G4.7 Sürüm 4.7.0:** CHANGELOG, sürüm notu (paket README'leri dahil), tag (kullanıcı onayı), doğrulama, taban 4.7.0.

### F5 — Geri bildirim döngüsü (sürekli) · ~0,5 oturum
- **G5.1** Issue şablonları: "Docs feedback", "Template problem" (`.github/ISSUE_TEMPLATE/`).
- **G5.2** Her belge sayfasının altında "Edit this page / Report a problem" (docfx `_gitContribute` zaten var; doğrula).
- **G5.3** Ölçüm güncellemesi: F1 + 30 gün ve F4 + 30 gün `adoption-metrics.md`'ye tarihli satır.

## 9. Başarı ölçütleri
| # | Ölçüt | Hedef | Nasıl ölçülür | Kapı mı |
|---|---|---|---|---|
| M1 | `dotnet new` → `/health/ready` 200 | ≤ 60 s (sıcak önbellek) | `template-smoke.yml` | Evet (ratchet) |
| M2 | Üretilen çözüm | 3 veritabanında 0 uyarı build, sqlite'ta testler yeşil | `template-smoke.yml` | Evet |
| M3 | Belge kalitesi | Türkçe 0, derlenmeyen C# bloğu 0, kırık bağlantı 0 | `docs_language.py`, snippets, docfx | Evet |
| M4 | Analyzer kesinliği | Yanlış pozitif 0 (repo içi) | G4.4 | Evet |
| M5 | Benimsenme (gecikmeli) | Taban + 30/60 gün eğilim | `adoption-metrics.md` | Hayır (gözlem) |

## 10. Etki haritası
- **Değişen:** `src/Kck.Templates/**` (içerik baştan), `src/Providers/Kck.Persistence.EntityFramework/*.csproj` (analyzer
  çıktısı), yeni `src/Analyzers/**`, `docs/**` (yeniden yapı + çeviri), `toc.yml`, `docfx.json`, `README.md`,
  `samples/Kck.Sample.WebApi` → `Kck.Sample.Orders`, `.github/workflows/{template-smoke,docs,build-test}.yml`,
  `.github/scripts/{docs_language,readme_snippets}.py`, `OmerkckArchitecture.sln`, `Directory.Packages.props`
  (Scalar, Roslyn paketleri), `.github/license-policy.json`, **yeni public API (eklemeli):**
  `Kck.Core.Abstractions` `IDomainEventHandler<TEvent>` ve `Kck.Persistence.EntityFramework`
  `KckPersistenceBuilder.AddDomainEventHandlers` → iki projenin `PublicAPI.Unshipped.txt`'si.
- **Referans veren:** README "Tam örnek" bağlantıları, `docs/README.md`, `CONTRIBUTING.md` (belge dili kuralı İngilizce
  olur), `docs/providers/README.md` bağlantıları, nuget README'leri, `nuget-publish.yml` (şablon paketleme +
  `KCK_PACKAGE_VERSION` damgası).
- **Tutarlı kalmalı:** paket doğrulaması (API kırılması yok — DX4), PublicAPI dosyaları (analyzer yeni public API
  eklemez), lisans kapısı, `readme_snippets` sözleşmesi, `rules/dotnet.md` (VSA), CLAUDE.md "warnings are errors".
- **Koruyan testler:** `template-smoke.yml` (yeni), `Kck.Persistence.EntityFramework.Tests` (300), README/belge snippet
  derlemesi, analyzer testleri (yeni), docfx derlemesi.
- **Tekrar kontrolü:** şablon testi (`grep -l "dotnet new" .github/workflows` → yalnız nuget-publish paketliyor, test yok);
  belge dil koruması yok; persistence sağlık kontrolü zaten var (`HealthChecks/`) → şablonda yeniden yazılmaz, kullanılır.
- **Proje belleği:** `docs/memory/architecture.md` yok → G0.1 ile birlikte kısa modül/sözleşme haritası oluşturulur.

## 11. Kapılar (Kurumsal + K3)
`/currency-check` (G1.2, G4.1) · örnek onayı (§4, bu planla) · `/planning` (bu belge) · TDD her görevde · `/code-review`
her PR · `/premortem` (§12) · `/blindspots` (§13) · `/adr` (0029, 0030, 0031) · `/adversarial-review` (G1.5 şablon güvenlik
varsayılanları; G4.2 kod düzeltmesi kullanıcı kodunu yeniden yazdığı için) · CI kalite kapısı · `/release` go/no-go her
sürümde (tag kullanıcı onayıyla).

## 12. Premortem (varsayım: plan başarısız oldu — neden?)
| # | Başarısızlık nedeni | Olasılık | Önlem (plandaki yeri) |
|---|---|---|---|
| R1 | Şablon testi PR'da çalışmıyor: şablon `4.x.y-alpha` Kck paketlerine başvuruyor, bunlar nuget.org'da yok | Yüksek | Yerel paket kaynağı + `nuget.config` (G0.3) |
| R2 | Şablon çıktısı değişince eski komut dizileri (`dotnet new kck-webapi` + proje yolu) kırılır | Orta | CHANGELOG "Changed — behaviour", sürüm notunda eski/yeni ağaç (G1.8) |
| R3 | SQLite: `decimal` sıralama/toplama sorguda hata; `[Timestamp]` üretilmez → eşzamanlılık koruması yok | Yüksek | G1.9 sorgu kuralı + belge + Postgres entegrasyon testi |
| R4 | 60 s kapısı runner gürültüsüyle rastgele düşer, ekip kapıyı kapatır | Orta | Aşamalı ölçüm, ratchet, p90 + pay (G1.6) |
| R5 | Şablon çıktısı CPM dışında; EF sağlayıcı / test paketi sürümleri elle yazılır ve eskir | Yüksek | Paketleme anında damgalama + yer tutucu kontrolü (G1.1) |
| R6 | `docs/` bloklarının çoğu parça kod; derleme kapısı ya hiç açılmaz ya muaf listesi şişer | Yüksek | Önce ölç, muaf sayısı yalnız azalır (G2.1) |
| R7 | docfx `--warningsAsErrors` bağlantıyla ilgisiz API belge uyarılarıyla düşer | Orta | Önce ölç; yalnız bağlantı uyarıları kapı, diğerleri ratchet (G2.1) |
| R8 | Analyzer, kullanıcının derleyicisinden yeni Roslyn'e başvurur → CS8032, analyzer sessizce yüklenmez | Orta | SDK 10.0.402 Roslyn'ini hedefle, yükleme testi (G4.2) |
| R9 | Şablon kimlik doğrulaması: bundle JWT'yi yalnız `Security:Jwt:Issuer` ile kuruyor; `dotnet user-jwts` uyumsuz → ya güvensiz anonim uç nokta ya 60 s akışı bozulur | Yüksek | G1.5 bilgi hamlesi + a/b/c karar sırası + adversarial-review |
| R10 | Bundle rate limiting'i testlerde 429 üretir | Orta | Test fabrikası kapatır (G1.9) |
| R11 | Derleme taramasındaki tip sırası işleyici sırası sözleşmesi sanılır | Düşük | Sıra sözleşme değil, belgelenir (G1.0) |
| R12 | KCK1001 event kullanmayan herkeste uyarı → `TreatWarningsAsErrors` derlemeleri kırılır, analyzer kapatılır | Orta | Bağlama duyarlı önem + kesinlik kapısı (G4.2, G4.4) |
| R13 | Site main'den yayınlanır, belge yayınlanmamış özelliği anlatır | Orta | F4 belgeleri sürüm PR'ıyla (G4.6a) |
| R14 | Çeviri (~2.200 satır) F2'yi aylarca uzatır, site yarı Türkçe kalır | Yüksek | Önce getting-started + persistence; dosya başına izin listesi; F2 oturum başına 3–4 dosya |
| R15 | Karşılaştırma sayfası rakip hakkında eskimiş/yanlış iddia içerir → güven kaybı | Orta | Tarihli kaynak dosyası, rakibin güçlü yanları da yazılır (G2.5) |
| R16 | Plan uzun; oturumlar arası bağlam kaybı (compaction) | Orta | `tasks/checkpoint.md` her görevden sonra; faz başına ayrı PR seti |

**Go / No-Go:** **Go.** Engelleyici risk yok. Zorunlu değişiklikler plana işlendi (R1, R3, R5, R9 yüksek olasılıklı ve
artık görev içinde). Tek gerçek belirsizlik R9 (kimlik doğrulama hikâyesi); G1.5'in ilk adımı bunu çözen bilgi hamlesi.

## 13. Kör noktalar
**Konuşulmamış gereksinimler → plana eklendi**
- **IDE'de görünürlük:** şablon Visual Studio / Rider "New Project" penceresinde parametreleriyle görünmeli →
  `.template.config/ide.host.json` + simge (G1.1).
- **Proje adı biçimleri:** `-n My.Shop`, `-n my-shop` geçerli namespace üretmeli → smoke matrisine iki ad (G1.1).
- **İşletim sistemi:** SQLite dosya yolu ve betikler Windows'ta da çalışmalı → `template-smoke.yml` ubuntu + windows (G1.1).
- **Negatif gereksinimler (şablon ne yapmamalı):** Docker istememeli; çıktı klasörü dışına dosya yazmamalı (SQLite dosyası
  içerik kökünde, `.gitignore`'da); varsayılan olarak ağa telemetri göndermemeli (OTLP uç noktası yapılandırılmadıkça
  dışa aktarım yok); sır üretmemeli (Ö7) → G1.5 testlerine eklendi: `Template_DefaultRun_NoOutboundTelemetry` (OTLP
  yapılandırması yok → exporter kayıtlı değil), `.gitignore` içeriği kontrolü.
- **Şablon güncelleme yolu:** eski şablonu kurmuş kullanıcı için `dotnet new update` / `dotnet new install
  Kck.Templates::4.6.0` sürüm notunda (G1.8).

**Verilmemiş ama verilmiş sayılan teknik kararlar → karara bağlandı**
- **.NET 11:** .NET 11 GA 2026-11'de bekleniyor, F1 ile aynı döneme düşebilir. Şablon `net10.0` (LTS) ile çıkar;
  `Framework` seçimine `net11.0` yalnız H19 GA yolu tamamlandıktan sonra eklenir (ayrı görev, bu planın dışında).
- **Belge sürümleme:** site tek sürüm (main). Sürüm başına site yok; yayınlanmamış özellik notu G4.6a ile.
- **Analyzer performansı:** `EnableConcurrentExecution` + üretilmiş kodu atlama; G4.4'te 58 projelik çözümde derleme
  süresi farkı ölçülür, > %5 ise kural daraltılır.
- **`AddKckWebApiDefaults` + `AddKckPersistence` birlikte:** örnek proje bunları ayrı ekliyor, bundle ile birlikte
  hiç denenmedi → G1.3 testi `Program_BundleAndPersistence_StartsAndResolvesUnitOfWork`.

**Süreç**
- **Onaylar:** plan (şimdi); ADR-0029 F0'da, ADR-0030 G1.0'dan önce, ADR-0031 G4.1'den önce (kullanıcı onayı); her sürüm
  tag'i ve `nuget` ortamı ayrı onay.
- **İngilizce metnin incelemesi:** kullanıcının ana dili Türkçe; İngilizce metin kalitesini korumak için stil rehberi
  (`docs/contributing/docs-style.md`, G2.2), dil koruması, derlenen kod ve her F2 PR'ında Türkçe özet. Kullanıcı anlamı
  ve davranışı onaylar, dil inceliğini değil.
- **Geri alma:** §17.

**Varsayım envanteri**
| Varsayım | Doğrulama | Kritiklik |
|---|---|---|
| `dotnet user-jwts` bundle'ın JWT yapılandırmasıyla uyumlu | G1.5 bilgi hamlesi | Yüksek |
| SDK 10.0.402'de `.slnx` ile `dotnet build/test` ve VS/Rider/VS Code açılışı sorunsuz | G1.1 smoke + elle açılış | Yüksek |
| docfx modern şablonu Diátaxis klasör gezinmesini istenen sırayla gösterir | G2.2 prototip | Orta |
| SQLite'ta global soft-delete filtresi + cascade, repo testlerindeki gibi davranır | Mevcut 300 test SQLite kullanıyor (kısmen doğrulandı); şablon testleri | Orta |
| Scalar.AspNetCore lisansı ve bakımı uygun | G1.2 currency-check | Orta |
| OpenTelemetry varsayılanı ağa çıkmaz | G1.5 testi | Orta |
| Çeviri anlamı korur | snippet derlemesi + davranış örnekleri + inceleme | Orta |
| Şablon motoru koşullu PackageReference + dosya dışlama | G1.1 smoke matrisi | Düşük |
| GitHub Pages dağıtımı ücretsiz ve çalışır kalır | docs.yml mevcut | Düşük |

## 14. İnceleme odağı
1. Şablonun güvensiz varsayılanı üretime taşıması (G1.5 testi + adversarial).
2. Commit'ten önce dağıtılan event (Ö2 testi `…AfterCommit`).
3. Analyzer yanlış pozitifinin kullanıcı derlemesini kırması (G4.4).
4. Belgelerdeki kod ile paketin gerçek API'sinin kayması (snippet derlemesi).
5. Karşılaştırma sayfasında doğrulanmamış rakip iddiası (G2.5 kaynak dosyası).

## 15. Sınırlar
- Aspire AppHost'lu şablon varyantı, TemplateVerifier anlık görüntü testleri, outbox modülü, çok kiracılılık: bu planın
  dışında; F5 geri bildirimine göre ayrı plan.
- Tanıtım (C seçeneği: blog, karşılaştırma yazıları, topluluk) bu planın dışında; F2.5 sayfası ileride kullanılabilir.
- GitHub repo açıklaması / konu etiketleri / Discussions: kullanıcı ayarı (§16).

## 16. Kullanıcının adımları
- Şimdi: planı onaylamak.
- F1 ve F4 sonunda: tag + `nuget` ortam onayı (her sürüm için ayrı onay).
- İsteğe bağlı (F5): GitHub repo açıklaması ve konular (`dotnet`, `entity-framework-core`, `soft-delete`,
  `domain-events`, `clean-architecture`, `dotnet-template`) — Settings → General; Discussions açmak — Settings → Features.

## 17. Geri alma
- **Şablon (4.6.0):** nuget.org silmez; `Kck.Templates` 4.6.0 unlist + önceki sürüm `dotnet new install
  Kck.Templates::4.5.0` ile kurulabilir. Düzeltme 4.6.1 olarak çıkar. Üretilmiş projeler etkilenmez.
- **Kütüphane API'si (G1.0):** eklemeli; geri almak kırıcı olur → hata varsa yama, kaldırma yalnız v5'te `[Obsolete]` ile.
- **Analyzer (4.7.0):** kullanıcı `.editorconfig` ile kapatabilir (`dotnet_diagnostic.KCK1001.severity = none`); bizim
  tarafımızda 4.7.1 ile önem düşürülür veya kural çıkarılır.
- **Belgeler:** git revert; site bir sonraki docs koşusunda düzelir.
