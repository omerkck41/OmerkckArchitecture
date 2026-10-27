# Dış İnceleme Karşılaştırması ve Geliştirme Yol Haritası — Plan

- Tarih: 2026-10-26 · Kademe: K3 (yol haritası; her faz kendi kademesinde yürür) · Profil: Kurumsal (TIER 3) · Onay: bekliyor
- Hedef: Ücretli lisans gerektiren bağımlılığı sıfırlamak ve bunu CI'da kalıcı (fail-closed) kılmak; ardından vadesi
  geçmiş temizliği v4.0.0'da yapıp kalite ve benimsenme işlerine geçmek.
- Kısıt (kullanıcı, 2026-10-26): projede ücretli lisans kullanılmaz.
- Girdi: dış AI raporu ("Kapsamlı Mimari, Sürdürülebilirlik ve Lisans Analiz Raporu", 26 Ekim 2026) + bu oturumun
  bağımsız incelemesi. Lisans yorumları hukuki görüş değildir.

## 1. Özet hüküm
- Dış rapor mimariyi doğru okumuş, fakat ana lisans riskini kaçırmış: yayındaki `Kck.Pipeline.MediatR` paketi
  **MediatR 14.1.0**'a (RPL-1.5 veya ticari lisans) bağlı ve CI lisans denetimi bunu göremiyor.
- Raporun iki temel önerisi yanlış veriye dayanıyor: net8.0 multi-target (.NET 8 EOL 2026-11-10) ve %43 kapsam
  (gerçek ölçüm %56 satır / %54,5 dal).
- ImageSharp riski abartılmış: Kck tüketicisi için geçişli bağımlılık olarak Apache-2.0; yük yalnız bakımcıda
  (Community lisans anahtarı, 2028-01-12'ye kadar).

## 2. Karşılaştırma (iddia → kanıt → hüküm)
| # | Dış rapor iddiası | Kanıt (bu oturum) | Hüküm |
|---|---|---|---|
| K1 | ImageSharp: tüketiciler ücret / denetim riskine maruz | Paket `LICENSE` (Split 1.0): "consuming the Work as a Transitive Package Dependency" → Apache-2.0; doğrulama hedefi yalnız `build/` (buildTransitive yok) | Yanlış (tüketici için); proje açık kaynak → Apache-2.0 |
| K2 | ImageSharp: anahtarsız Release derlenemez | `SixLabors.ImageSharp.targets`: `ContinueOnError` yalnız Debug'da | Doğru (CONTRIBUTING notu zaten var) |
| K3 | SkiaSharp varsayılan sağlayıcı olsun | `decisions/2026-10-14-...imagesharp-4.md`: GIF/BMP yazamaz, Linux native paket ister; sağlayıcı GIF/BMP destekliyor (`ImageSharpProcessor.cs:71-72`) | Önceki kararla çelişiyor; özellik kaybı |
| K4 | Hangfire: LGPL + Pro riski | Hangfire `LICENSE.md`: LGPL-3.0 veya ticari, alıcı seçer; Pro paketi kullanılmıyor | Doğru; kaçırılan: WorkerService bundle varsayılanı Hangfire (`KckWorkerServiceDefaults.cs:12`) |
| K5 | Elasticsearch sunucu lisansı | İstemci `Elastic.Clients.Elasticsearch` Apache-2.0 | Doğru sınıflama; Kck'de aksiyon gerekmez |
| K6 | SaaS ücretli; MinIO / Vault / UserSecrets alternatif | `AwsS3StorageService.cs:110-121` yalnız `RegionEndpoint`; UserSecrets yalnız geliştirme içindir | Sınıf doğru; MinIO önerisi bugün çalışmaz (özel endpoint yok); UserSecrets üretim alternatifi değil |
| K7 | net8.0 multi-target eklensin | endoflife.date (erişim 2026-10-26): .NET 8 ve .NET 9 EOL 2026-11-10, .NET 10 LTS EOL 2028-11-14; ADR-0019 | Yanlış (15 gün sonra EOL; EF Core 10 / ASP.NET Core 10 net8'i desteklemez) |
| K8 | Kapsam %43 / %38 | CI `build-test` main, 2026-10-15: satır %56, dal %54,5 (784/1437), metot %60,7 | Yanlış (CI eşiği okunmuş, ölçüm değil) |
| K9 | xUnit v2 borcu | `dotnet list package --deprecated`: xunit 2.9.3 "Legacy" → xunit.v3 | Doğru (Stryker engeli yeniden doğrulanmalı) |
| K10 | MediatR KCK0200 ile başarıyla deprecate edildi | `Directory.Packages.props:29` MediatR 14.1.0; paket `LICENSE.md` RPL-1.5 / Lucky Penny, `requireLicenseAcceptance=true`; README: anahtarsız çalışma anı uyarısı; ADR-0021 "v3.0+'da kaldırılacak" → v3.5.0'da yayında | **Kaçırılmış — en kritik bulgu** |
| K11 | (değinilmemiş) CI lisans denetimi | `license-audit.yml:50-62`: yalnız GPL/LGPL/SSPL/BUSL dizgesi aranıyor; dosya lisanslı paketler görünmüyor; `\|\| true` fail-open; `nuget-license` sürümsüz kuruluyor; son çalışma 2026-10-26 "OK: no copyleft" (Hangfire LGPL olduğu hâlde) | **Kaçırılmış** |
| K12 | Deprecation politikası 9,5/10 | `deprecation.md` tablosunda yalnız KCK0001 (hedef v1.0) — kod v3.5'te hâlâ; KCK0100 (6 metot), KCK0200, EventBus.InMemory (2 metot, DiagnosticId yok) tabloda yok; yönlendirme tipleri politikaya aykırı olarak minor v3.2'de kaldırılmış | Abartılı (uyarı tarafı var, kaldırma tarafı işlemiyor) |
| K13 | Dokümantasyon 9,5/10 | `Kck.Documents.Abstractions/README.nuget.md` Quick Start: 2 yanlış DI adı, yanlış parametre adı, 2 dönüş tipi uyumsuzluğu, olmayan `ResizeAsync` overload'u; yapılandırma tablosunda kodda olmayan 2 anahtar | Abartılı (belge sayısı ≠ doğruluk) |
| K14 | Public API disiplini 9,5/10 | PublicApiAnalyzers yalnız 17 Abstractions projesinde; 40+ sağlayıcı/bundle'da yok; `EnablePackageValidation` hiç yok | Kısmen doğru |
| K15 | (değinilmemiş) Bağımlılık tazeliği | Bekleyen major: Quartz 4.6.0, StackExchange.Redis 3.4.20, OpenTelemetry 1.20.1, YamlDotNet 18.1.0, FluentFTP 55.1.0, MinVer 8.0.0; 8 Dependabot NuGet PR'ı bir haftadır açık | Kaçırılmış |
| K16 | (değinilmemiş) Güvenlik ve tedarik zinciri | `--vulnerable --include-transitive`: 0 açık; Actions SHA pin, zizmor, Trusted Publishing, `nuget` ortam onay kapısı | Kaçırılmış (olumlu) |
| K17 | Abstractions dış teknolojiden bağımsız | Yalnız `Microsoft.Extensions.DependencyInjection/Logging.Abstractions` | Doğru |
| K18 | "ORM değişimi tek satır DI ile" | Tek persistence sağlayıcısı (EF Core) | Teorik |
| K19 | ImageSharp açıklaması (rapor değinmemiş) | `.csproj` açıklaması AVIF vadediyor, kodda AVIF kodlayıcı yok; bilinmeyen format sessizce JPEG'e düşüyor (`ImageSharpProcessor.cs:74`) | Kaçırılmış (küçük) |

## 3. Lisans envanteri (lock dosyaları + nuspec taraması, 2026-10-26)
- `src` projelerinin çözümlenmiş bağımlılıkları (151): 104 MIT · 30 Apache-2.0 · 3 BSD-3-Clause ·
  1 Apache-2.0 OR MPL-2.0 (RabbitMQ.Client) · 2 URL (MIT) · 11 dosya lisanslı.
- Ticari şartlı: **MediatR 14.1.0** (RPL-1.5 veya Lucky Penny; 12.5.0 son Apache-2.0 sürüm — yerel nuspec ile
  doğrulandı) · **SixLabors.ImageSharp 4.1.3** (Split 1.0; Kck tüketicisine geçişli Apache-2.0).
- Ücretsiz copyleft: Hangfire ailesi 1.8.25 (6 paket; LGPL-3.0 seçeneği).
- Dosya lisanslı ama izin verici: Mediator.Abstractions (MIT), Otp.NET (MIT), Apache.Arrow (Apache-2.0).
- Test/geliştirme bağımlılıkları: tümü izin verici. Güvenlik açığı: 0.

## 4. Alternatifler
| Yaklaşım | Artı | Eksi | Karar |
|---|---|---|---|
| A. Dış raporun yol haritası (SkiaSharp varsayılan, net8, OpenSearch, Coravel, Analyzer) | Seçenek genişliği | Ana ücretli riski (MediatR) çözmez; net8 EOL; tek bakımcıya 4+ yeni paket | Elendi |
| B. Önce kalıcı kapı, sonra temizlik (fail-closed lisans kapısı → v3.6 → v4.0 temizlik) | Kök nedeni çözer; FluentAssertions / MediatR / MassTransit örüntüsündeki gelecek lisans değişimlerini yakalar | v4.0 major gerektirir | **Seçildi** |
| C. Yalnız belge (README uyarıları) | Kırıcı değişiklik yok | Ücretli bağımlılık yayında kalır; kısıtı ihlal eder | Elendi |

Alt kararlar:
- **Lisans kapısı aracı** (H1'de currency-check ile kesinleşir): (1) depo içi betik — lock + nuspec + politika dosyası,
  fail-closed; bu oturumda aynı yöntem 3 sorunlu aileyi buldu · (2) `nuget-license` sabit sürüm + allowlist + override ·
  (3) GitHub dependency-review — yalnız PR'da çalışır, dosya lisans verisi belirsiz. Öneri: (1), Plan B: (2).
- **MediatR**: (1) v4.0.0'da kaldır + v3.6'da README/açıklama uyarısı + nuget.org deprecation · (2) v3.6'da yayını
  durdur — politika kaldırmayı major'a bağlıyor (`deprecation.md:12-13`) · (3) MediatR 12.5.0'a sabitle — 14 → 12 ikili
  uyumu belirsiz, donmuş sürüm yama almaz. Öneri: (1).

## 5. Hamle haritası v1
Derinlik: K3 · Kapsam dışı: §9 Sınırlar

### F0 — Lisans kapısı ve hijyen (K2; yerel + CI, geri alınabilir; 2 PR)
| # | Hamle | Çıktı | Bitti kanıtı (DoD) | Bağımlı | R | Etki | Onay |
|---|---|---|---|---|---|---|---|
| H1 | Lisans kapısı aracı seçimi (currency-check, 2 alternatif) | `decisions/2026-10-xx-license-gate.md` | Karar dosyası: kaynak + tarih + sürüm + gerekçe | - | R3 | Düşük | harita |
| H2 | Fail-closed lisans kapısı + `docs/licensing.md` (izinli liste, istisnalar: ImageSharp ve Hangfire kalıcı, MediatR vadeli geçici) + CONTRIBUTING'e "yeni lisans istisnası" adımı | `license-audit.yml`, politika dosyası | E1 FAIL → E2 PASS → E3 FAIL; adversarial-review 1 tur (güvenlik kapısı) | H1 | R3 | Orta | harita |
| H3 | Hijyen: kapsam eşiği 43/38 → 55/53; ölü `PackageVersion` girdileri (NetAnalyzers preview, System.Drawing.Common); Dependabot `testing` grubu FluentAssertions → AwesomeAssertions; `deprecation.md` tablosu gerçek listeyle (hedef v4.0); `known-issues.md` K1 | 5 dosya | E4; solution geneli `restore --force-evaluate` + `--locked-mode` + Release build 0 hata | - | R3 | Düşük | harita |
| H4 | Belge düzeltmeleri: Documents.Abstractions README; ImageSharp açıklaması (AVIF yok, GIF/BMP var); Pipeline.MediatR README/açıklamasına lisans uyarısı; MinimalApi açıklaması | 4-5 dosya | E5 | - | R3 | Düşük | harita |
| H5 | Dependabot birikimi (8 NuGet + 2 Actions PR) | Birleşmiş PR'lar | Her PR'da CI yeşil (yeni lisans kapısı dahil) | H2 | R2 | Orta | ayrıca (merge) |

### F1 — v3.6.0 minor (K2; 2 PR + yayın)
| # | Hamle | Çıktı | Bitti kanıtı (DoD) | Bağımlı | R | Etki | Onay |
|---|---|---|---|---|---|---|---|
| H6 | S3 uyumlu özel endpoint: `AwsS3Options.ServiceUrl`, `ForcePathStyle` (+ checksum modu; AWS SDK v4 currency-check) | AwsS3 sağlayıcı + testler | Birim test: options → `AmazonS3Config` değerleri; mevcut testler yeşil | - | R3 | Orta | harita |
| H7 | Paket doğrulama: `EnablePackageValidation`, taban 3.5.0 (paketlenen projeler) | `Directory.Build.props` | `dotnet pack -c Release` yeşil; kasıtlı public metot silme denemesi pack'i düşürür (RED) | - | R3 | Orta | harita |
| H8 | Unlist adımına major koruması: önceki major'ın en yeni kararlı sürümü listede kalır | `nuget-publish.yml` | Örnek sürüm listesiyle sınama: 4.0.0 yayınında 3.6.x listede kalır, 3.6.1 yayınında 3.6.0 unlist olur; zizmor + actionlint temiz | - | R2 | Yüksek | ayrıca |
| H9 | v3.6.0 yayını (sürüm notunda "4.0'da kaldırılacaklar" duyurusu) | nuget.org 3.6.0 | 59 paket yayında + unlist raporu | H2-H8 | R0 | Yüksek | ayrıca (tag) |
| H10 | (senin adımın) nuget.org'da `Kck.Pipeline.MediatR` → Deprecated, alternatif `Kck.Pipeline.Mediator` | Paket sayfası | Paket sayfasında deprecation bandı | H9 | R2 | Orta | ayrıca |

### F2 — v4.0.0 temizlik major'ı (K3: premortem + ADR + adversarial-review; 1-2 PR + yayın)
| # | Hamle | Çıktı | Bitti kanıtı (DoD) | Bağımlı | R | Etki | Onay |
|---|---|---|---|---|---|---|---|
| H11 | Major bağımlılık değerlendirmesi: Quartz 4.6, StackExchange.Redis 3.4 (↔ `Microsoft.Extensions.Caching.StackExchangeRedis` uyumu), OpenTelemetry 1.20, YamlDotNet 18, FluentFTP 55; xUnit v3 / Stryker engeli yeniden kontrol | Karar dosyaları | Her biri için currency-check artefaktı; Kck public API'sine yansıyanlar v4 kapsamına | - | R3 | Düşük | harita |
| H12 | v4 kapsam ADR'si (D2-D4 kararlarıyla) + `docs/memory/architecture.md` (modül bağımlılık haritası) | `docs/adr/0028-*.md` | Kullanıcı onaylı ADR | H11, D2-D4 | R3 | Orta | ayrıca |
| H13 | `Kck.Pipeline.MediatR`'ı kaldır (27 dosya: src, tests, sln, docs, CHANGELOG, sonar, KCK0200) + geçici lisans istisnasını sil | | Lisans kapısı MediatR istisnası olmadan yeşil; `Pipeline.MediatR` grep sonucu yalnız CHANGELOG/geçiş rehberi | H12 | R3 | Yüksek | harita |
| H14 | Vadesi geçmiş `[Obsolete]` API'leri kaldır: KCK0001 (`GetClaimsFromToken` — imza doğrulamaz), KCK0100 (6 metot), EventBus.InMemory eski kayıt metotları (2) | PublicAPI dosyaları | Release build 0 hata; testler yeşil; PublicAPI.Shipped güncel | H12 | R3 | Yüksek | harita |
| H15 | WorkerService bundle: varsayılan zamanlayıcı Quartz (D3'e göre Hangfire referansı da çıkar) | | D3=a: bundle lock dosyasında Hangfire yok | H12 | R3 | Orta | harita |
| H16 | ImageSharp: bilinmeyen format → `NotSupportedException` ("jpg" açık eşleme) | | Test: "avif" → `NotSupportedException`; "jpg" → JPEG imzası | H12 | R3 | Düşük | harita |
| H17 | `docs/migrations/v3-to-v4.md` + CHANGELOG + paket doğrulama bastırma dosyası (kasıtlı kırılmalar) | | Her kaldırma için önce/sonra örneği | H13-H16 | R3 | Orta | harita |
| H18 | v4.0.0 yayını | nuget.org 4.0.0 | 3.6.x listede kalır (H8 kanıtı) | H17, H8 | R0 | Yüksek | ayrıca (tag) |

### F3 — Sürekli kalite ve benimsenme (K2; iş başına 1 PR)
| # | Hamle | Çıktı | Bitti kanıtı (DoD) | Bağımlı | R | Etki | Onay |
|---|---|---|---|---|---|---|---|
| H19 | .NET 11 GA sonrası CI smoke (net10 testleri net11 runtime'da roll-forward ile; `global.json` değişmez) | `build-test.yml` | Yeşil iş | .NET 11 GA | R3 | Düşük | harita |
| H20 | Mutation matrisi: Observability, FileStorage, Http.Resilience, BackgroundJobs, Documents, Authorization; kapsam hedefi 65/60 | `stryker-*.json` | Modül başı skor raporu | - | R3 | Düşük | harita |
| H21 | README örneklerini derleme kontrolüne al (önce bundle'lar ve çok indirilen paketler) | Örnek derleme projesi | CI'da derlenir | H4 | R3 | Düşük | harita |
| H22 | AOT hedefli sağlayıcılarda `IsAotCompatible` + AOT publish smoke (Pipeline.Mediator, Caching.InMemory …) | | AOT/trim uyarısı 0 | - | R3 | Düşük | harita |
| H23 | Benimsenme odağı kararı (2026-05-30'dan açık; öneri DX) | | Kullanıcı kararı | D5 | - | - | ayrıca |

- Senin adımların: D1-D5 kararları (§8) · PR birleştirme onayları (H5) · yayın onayları (H9, H18: tag + `nuget`
  ortamı) · H10 nuget.org deprecation (web arayüzü; Trusted Publishing'de saklı API anahtarı yok).
- Tahmini çıktı: F0 2 PR · F1 2 PR + 1 yayın · F2 1-2 PR + 1 yayın · F3 iş başına 1 PR. Harici bekleme: CI,
  nuget.org indeksleme, .NET 11 GA.
- Plan B: H1'de seçilen araç dosya lisanslarını çözemezse → diğer alternatif; StackExchange.Redis 3 uyumsuzsa → 2.x'te
  kal. Plan C: v4 hazırlığında beklenmeyen kırılma → v4'ü durdur, 3.x'te kal; F0/F1 kazanımları bağımsızdır.
- Sıradaki 3 hamle: H1 → H2 → H3 (H4 bağımsız; H3 ile aynı PR'a girebilir).

## 6. Etki haritası (F0 ayrıntılı; sonraki fazlar kendi planında)
- Değişen: `.github/workflows/license-audit.yml`; politika dosyası (yer H1'de) ve gerekirse betik; `docs/licensing.md`
  (yeni); `CONTRIBUTING.md`; `.github/workflows/build-test.yml:147-148`; `Directory.Packages.props:20,130`;
  `.github/dependabot.yml` (testing grubu); `docs/policies/deprecation.md:54-56`; `docs/memory/known-issues.md:8`;
  `src/Abstractions/Kck.Documents.Abstractions/README.nuget.md`; `src/Providers/Kck.Documents.ImageSharp/*.csproj:4`;
  `src/Providers/Kck.Pipeline.MediatR/README.nuget.md` + `.csproj:4`; `src/Bundles/Kck.Bundle.MinimalApi/*.csproj:6`.
- Referans veren: CI iş akışları (eşik adımı), Dependabot PR'ları (license-audit tetiklenir).
- Tutarlı kalmalı: `decisions/2026-10-14-sdk-10.0.402-imagesharp-4.md`, ADR-0019, ADR-0021, deprecation politikası,
  CPM + lock kuralı (proje CLAUDE.md), "ücretli lisans yok" kısıtı.
- Koruyan testler: E1-E5; `dotnet build -c Release`; ölü `PackageVersion` silme için solution geneli restore doğrulaması.
- Tekrar kontrolü: mevcut `license-audit.yml` değiştirilir (yeni iş akışı açılmaz); lisans taraması için bu oturumdaki
  tarama yöntemi (lock + nuspec) başlangıç noktası.

## 7. Onaylı örnekler (F0 — Kabul Kapısı adım 1; kullanıcı onayı bekliyor)
- E1: Mevcut main'de kapı istisnasız çalışır → FAIL; çıktı MediatR 14.1.0, SixLabors.ImageSharp 4.1.3 ve Hangfire*
  paketlerini adıyla listeler.
- E2: Politika istisnalarıyla (ImageSharp, Hangfire kalıcı; MediatR v4.0'a kadar vadeli) → PASS.
- E3: Lisans verisi okunamayan veya izinli listede olmayan paket → FAIL (fail-closed), paket adı + "UNKNOWN".
- E4: Kapsam eşiği 55/53 → mevcut ölçüm (56 / 54,5) PASS; eşik altına düşen PR FAIL.
- E5: `Kck.Documents.Abstractions` README Quick Start'ı geçici bir projede derlenir → 0 hata.

## 8. Kullanıcı kararları
> Durum: D1 onaylandı 2026-10-26; D0, D2–D6 önerilen seçenekle onaylandı 2026-10-27 (kullanıcı).

| # | Karar | Seçenekler | Öneri |
|---|---|---|---|
| D0 | "Ücretli lisans yok" kısıtının kapsamı | (a) proje + tüketicinin varsayılan kullanım yolu · (b) yalnız projenin kendisi | (a) — benimsenme vaadi "varsayılan yol tamamen ücretsiz ve izin verici" olur; D2/D3 buna göre |
| D1 | Yol haritası (Yaklaşım B), F0 ile başlama ve lisans politikası: izinli liste (MIT, Apache-2.0, BSD-2/3, MS-PL, ISC, PostgreSQL; çift lisansta izin verici seçenek) + belgeli istisnalar (Hangfire LGPL-3.0, ImageSharp Split; MediatR yalnız v4.0'a kadar vadeli) | evet / önce tartış | evet |
| D2 | ImageSharp | (a) kalsın + lisans belgesi · (b) ek MIT sağlayıcı (SkiaSharp; GIF/BMP yazamaz) · (c) kaldır | (a) — tüketiciye Apache-2.0, kod tarafı sorunsuz |
| D3 | WorkerService varsayılan zamanlayıcı | (a) varsayılan Quartz + Hangfire referansı bundle'dan çıkar · (b) yalnız varsayılan Quartz · (c) mevcut hâl | (a) — bundle'da LGPL sıfırlanır, Hangfire isteyen paketi ayrıca ekler |
| D4 | v4.0.0 zamanı | (a) F0 + F1 sonrası, Kasım-Aralık 2026 · (b) 2027 başı | (a) |
| D5 | Benimsenme odağı (2026-05-30'dan açık) | (a) DX · (b) kahraman modül · (c) tanıtım | (a) — K13 bulgusu ilk izlenimin kırıldığını gösteriyor |
| D6 | `docs/plans`, `docs/archive`, `docs/memory` git'te izlenmiyor | commit / .gitignore | commit (proje hafızası kaybolmasın) |

## 9. Sınırlar (kapsam dışı / ertelenen)
- net8.0 multi-target: Red (K7). .NET 11 için multi-target gerekmez; net10.0 kütüphaneleri net11'de çalışır (H19 smoke).
- SkiaSharp varsayılan: Red (K3); D2(b) seçilirse opsiyonel ikinci sağlayıcı.
- Coravel: Red — Quartz (Apache-2.0) aynı işi görüyor.
- OpenSearch sağlayıcısı: Ertelendi — lisans sorununu çözmez (ES istemcisi Apache-2.0); talep gelirse.
- Kck.Analyzers (Roslyn): Ertelendi — maliyet yüksek, getirisi belirsiz.
- Tüm sağlayıcılarda AOT: H22'ye daraltıldı (EF Core, Hangfire, Newtonsoft.Json AOT uyumsuz).
- EventBus.InMemory `[Obsolete]`'lerine sonradan DiagnosticId eklenmez: uyarı kimliği CS0618'den değişince bunu susturan
  tüketicilerin derlemesi kırılabilir; v4'te kaldırılır.
- Tek bakımcı riski (59 paket): kod değil yönetişim konusu; D5 ile birlikte destek kademesi (Core / Extended) politikası
  ayrıca değerlendirilebilir.
- MediatR Community edition koşulları (<5M USD gelir) üçüncü taraf kaynaklı; karar buna dayanmıyor.

## 10. Premortem (plan başarısız oldu varsayımıyla)
| Risk | Olasılık | Önlem |
|---|---|---|
| Fail-closed kapı dosya lisanslı her yeni pakette PR'ları kırar → kapı yeniden gevşetilir | Yüksek | Override listesi + CONTRIBUTING adımı; E1-E3 ile RED/GREEN kanıtı |
| v4.0.0 yayını 3.x'in en yeni sürümünü unlist eder | Kesin (mevcut kod: `nuget-publish.yml:204-207`) | H8, v4'ten önce |
| StackExchange.Redis 3 ↔ `Microsoft.Extensions.Caching.StackExchangeRedis` 10.0.x ikili uyumsuzluk | Orta | H11'de ayrı değerlendirme; uyumsuzsa 2.x |
| Kaldırılan `[Obsolete]` API'ler örnekleri / şablonu kırar | Orta | samples + `Kck.Templates` CI'da derlenir; grep etki haritası |
| `Kck.Pipeline.MediatR` 3.x + Kck 4.x karışımı çalışma anında kırılır | Orta | Geçiş rehberinde "önce Kck.Pipeline.Mediator'a geç" |
| ImageSharp format sıkılaştırması çalışma anında kırar (derleme uyarısı yok) | Düşük | Yalnız v4; sürüm notu + desteklenen formatları listeleyen hata mesajı |
| AWS SDK v4 varsayılan checksum'ları S3 uyumlu sunucularda reddedilir | Orta | H6'da currency-check; checksum modu seçeneği |
| Tek bakımcı bant genişliği: Dependabot birikimi büyür | Yüksek | H5 temizliği; patch güncellemeleri için otomatik birleştirme koşullarını gözden geçir |
| Six Labors Community anahtarı yenilenmez | Düşük | known-issues K1 (2027-10-14); Plan B = D2(b) |

Go / No-Go: F0 **Go** (yerel, geri alınabilir). F2 **koşullu Go**: H8 + H11 + D2-D4 tamamlanmadan v4.0.0 yayını yok.
Blocker yok.

## 11. Tehdit modeli · Bitiş tanımı · Tur bütçesi
- Tehdit modeli — kapsam içi: Dependabot güncellemesiyle ticari lisansın sessizce girmesi; CI kapısının hata verip
  geçmesi (fail-open); yayın otomasyonunun önceki major'ı gizlemesi; kırıcı değişikliğin habersiz yayını; README'den
  kopyalanan kodun derlenmemesi. Kapsam dışı: tüketicilerin kendi doğrudan bağımlılıkları, bulut hizmet ücretleri,
  hukuki yorum.
- Bitiş tanımı: (1) lisans kapısı fail-closed ve main yalnız belgeli istisnalarla yeşil; (2) v4.0.0'da ücretli lisans
  gerektiren bağımlılık 0, şartlı lisanslı bağımlılık yalnız D2 kararıyla ve belgeli; (3) E1-E5 geçer; (4) kapsam içinde
  açık KRİTİK/YÜKSEK bulgu yok.
- Tur bütçesi: faz başına inceleme en fazla 2 tur. Süre tahmini: F0 1-2 oturum · F1 1-2 oturum · F2 2-3 oturum ·
  F3 sürekli. Bütçe dolunca dur → açık bulgular kullanıcıya.

## 12. Kör noktalar ve varsayım envanteri (/blindspots)
- Etki verisi (nuget.org arama API'si, 2026-10-26): 58 paket, toplam ~70,6 bin indirme, paket başı medyan ~1,2 bin;
  `Kck.Pipeline.MediatR` 1.136, `Kck.Pipeline.Mediator` 1.126. Dağılımın bu kadar düz olması indirmelerin çoğunun
  otomatik (ayna / tarayıcı) trafik olduğunu düşündürüyor (çıkarım, kanıt değil) → v4 kırıcı değişikliklerinin gerçek
  kullanıcı etkisi büyük olasılıkla düşük; benimsenme tezini (sorun bilinirlik) de destekliyor.

| Varsayım | Doğrulama | Kritiklik |
|---|---|---|
| Kck tüketicisi ImageSharp'ı geçişli kullanır → Apache-2.0 | Lisans metni okundu (K1); ImageSharp'ı doğrudan kullanan tüketici kendi değerlendirmesini yapar → `docs/licensing.md`'ye yazılır | Orta |
| Lock + nuspec taraması Linux CI'da da aynı sonucu verir | H2'de CI çalışması (E1-E3) | Orta |
| StackExchange.Redis 3, `Microsoft.Extensions.Caching.StackExchangeRedis` 10.0.x ile uyumlu | H11 | Yüksek (Caching.Redis, Caching.Hybrid, TokenBlacklist.Redis) |
| Kaldırılacak API'leri samples / şablon kullanmıyor | H13-H14 grep + CI derlemesi | Orta |
| nuget.org deprecation web arayüzünden yapılabilir | H10 sırasında | Düşük |
| .NET 11 Kasım 2026'da GA olur | H19 GA'yı bekler | Düşük |

- Yayın geri dönüşü (R0): nuget.org silmez → hatalı sürümde unlist + düzeltme yaması (x.y.z+1); gerekirse önceki
  sürüm relist edilir. Yayın öncesi go/no-go: CI yeşil, sürüm notu hazır, v4 için geçiş rehberi hazır.
- Başarı ölçütü (F3 / D5): D5 seçilince birlikte belirlenir; indirme sayısı tek başına yanıltıcı (yukarıdaki veri).

## Kaynaklar (erişim 2026-10-26)
- https://endoflife.date/api/dotnet.json
- SixLabors.ImageSharp 4.1.3 paket `LICENSE` ve `build/SixLabors.ImageSharp.targets` (yerel NuGet önbelleği)
- MediatR 14.1.0 / 12.5.0 paket `LICENSE.md`, `README.md`, `mediatr.nuspec` (yerel NuGet önbelleği)
- https://www.jimmybogard.com/automapper-and-mediatr-commercial-editions-launch-today/
- Hangfire 1.8.25 paket `LICENSE.md` (yerel NuGet önbelleği)
- GitHub Actions: build-test main 2026-10-15 (run 38658902306), license-audit 2026-10-26 (run 40324570972)
