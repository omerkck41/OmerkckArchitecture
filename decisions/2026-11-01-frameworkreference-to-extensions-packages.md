# Karar: `FrameworkReference Microsoft.AspNetCore.App` yerine `Microsoft.Extensions.*` paketleri (F3, K5)

- Tarih: 2026-11-01
- Bağlam: `docs/plans/2026-10-31-provider-saglik-ve-atil-kod-plani.md` K5 / F3 (D6: 5.0'da). 31 paketlenebilir proje
  `<FrameworkReference Include="Microsoft.AspNetCore.App" />` taşıyor; çoğu yalnız `Microsoft.Extensions.*` API'si kullanıyor.
  Çerçeve referansı nuspec'e `<frameworkReference>` olarak akıyor → worker/console tüketicisi ASP.NET Core paylaşımlı
  çerçevesine bağlanıyor (`dotnet/runtime` imajında çöker). Kanıt: nuget.org `kck.caching.inmemory` 4.8.0 nuspec (plan §1 K5).
- Pre-commitment (araştırmadan önce beyan): aday `Microsoft.Extensions.*` 10.0.x paketleri, CPM'deki mevcut 10.0.14 hattı;
  doğrulama kaynakları nuget.org flatcontainer + endoflife.date; eleme kriterleri: paket kimliği resmi kayıtta birebir var mı,
  10.x hattında en son kararlı sürüm mü, .NET 10 EOL, `rules/deprecated-registry.md` eşleşmesi. Not: sürüm sorgusu beyanla aynı
  turda yapıldı; aday kümesi plan kararından (2026-10-31) geliyor.
- Registry taraması (Anayasa #6): `Microsoft.Extensions`, `FrameworkReference`, `AspNetCore.App` için kayıt yok.

## Adaylar

| Aday | Açıklama | Karar |
|---|---|---|
| A — Durum korunur | 31 projede `FrameworkReference` kalır | Elendi: K5 bulgusu çözülmez; worker/console tüketicisi ASP.NET Core çerçevesine bağlı kalır |
| B — `Microsoft.Extensions.*` paketleri (10.0.14) | ASP.NET Core tipi kullanmayan projelerde çerçeve referansı kaldırılır, kullanılan soyutlamaların paketleri açık referans olur; gerçekten `Microsoft.AspNetCore.*` kullananlar (örn. `Kck.AspNetCore`, `Kck.Exceptions.AspNetCore`, WebApi/MinimalApi bundle'ları) çerçeve referansını korur | **Seçildi** |
| C — Çoklu hedef (`net10.0` + `net10.0` ASP.NET varyantı) veya koşullu `FrameworkReference` | Tüketici seçsin | Elendi: NuGet'te aynı TFM için iki varyant yok; paket sayısı ikiye katlanır; B zaten ihtiyaca göre ayırıyor |
| D — `Microsoft.Extensions.*` 9.0.x | Daha geniş uyumluluk | Elendi: hedef `net10.0`, çerçeve 10.0 ile uyumsuz sürüm karışımı (NU1605/CS0433 riski); 9.0 EOL 2026-11-10 |

## Doğrulama (Anayasa #1, #5)

- nuget.org flatcontainer (`https://api.nuget.org/v3-flatcontainer/<id>/index.json`, erişim 2026-11-01): aşağıdaki kimliklerin
  hepsi mevcut, en son kararlı sürüm **10.0.14** (10.x hattı ve genel): `Microsoft.Extensions.Caching.Memory`,
  `Microsoft.Extensions.Caching.Abstractions`, `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Configuration.Abstractions`,
  `Microsoft.Extensions.Configuration.Binder`, `Microsoft.Extensions.Options.ConfigurationExtensions`,
  `Microsoft.Extensions.Options.DataAnnotations`, `Microsoft.Extensions.Diagnostics.HealthChecks`,
  `Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions`, `Microsoft.Extensions.Http`, `Microsoft.Extensions.Localization`,
  `Microsoft.Extensions.Localization.Abstractions`, `Microsoft.Extensions.Hosting.Abstractions`, `Microsoft.Extensions.Logging.Abstractions`,
  `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Options`, `Microsoft.Extensions.Primitives`,
  `Microsoft.Extensions.FileProviders.Abstractions`. CPM'deki mevcut Microsoft.Extensions.* satırları da 10.0.14 → tek sürüm hattı.
- endoflife.date (`https://endoflife.date/api/dotnet.json`, erişim 2026-11-01): .NET 10 LTS, EOL **2028-11-14**; .NET 9 EOL 2026-11-10,
  .NET 8 EOL 2026-11-10. (endoflife "latest 10.0.13" gösterir, nuget 10.0.14'ü yayınlamış — kayıt gecikmesi.)
- Yayıncı: Microsoft (`dotnet/runtime`, `dotnet/extensions`); lisans MIT. Son dönem sinyali gerekmedi (BCL/çerçeve paketleri,
  Microsoft imzalı; `.github/license-policy.json` izinli liste MIT).
- Hangi projenin hangi paketi alacağı: F3 uygulama turunda `grep` tabanlı eşleme (ADR-0032 ve PR gövdesi); yalnız kodun
  gerçekten kullandığı soyutlama paketleri eklenir, ASP.NET Core tipi kullanan projeler çerçeve referansını korur.

## Keşif sonucu eklenen paket değişimleri (2026-11-01, salt okuma analizi — 31 proje: 5 KEEP, 26 REPLACE)

İki sağlayıcının mevcut paketi ASP.NET Core çerçeve referansını kendi nuspec'inde taşıyor; çerçeve referansını kaldırmak tek başına
etkisiz kalır:

| Sağlayıcı | Mevcut | Yeni | Doğrulama (nuget.org flatcontainer + `gh api`, 2026-11-01) |
|---|---|---|---|
| `Kck.BackgroundJobs.Hangfire` | `Hangfire` 1.8.25 (metapaket → `Hangfire.AspNetCore` → çerçeve) | `Hangfire.Core` 1.8.25 + `Hangfire.NetCore` 1.8.25 | kimlikler mevcut, en son kararlı 1.8.25 (metapaketle aynı); repo HangfireIO/Hangfire aktif (push 2026-10-30, v1.8.25 2026-08-28), lisans `NOASSERTION` = LGPL-3.0 / ticari ikili — `.github/license-policy.json` `Hangfire*` belgeli istisnası kapsar (SHA listesi kapı koşusunda doğrulanır) |
| `Kck.Logging.Serilog` | `Serilog.AspNetCore` 10.0.0 (nuspec'te çerçeve referansı) | `Serilog.Extensions.Hosting` 10.0.0 + `Serilog.Sinks.Console` 6.1.1 + `Serilog.Sinks.File` 7.0.0 | kimlikler mevcut, en son kararlı sürümler (Serilog.AspNetCore'un sabitlediği sürümler); repolar aktif, Apache-2.0 |

Kırıcı etki (geçiş rehberine): tüketici artık `Hangfire.SqlServer`'ı ve Serilog'un `UseSerilogRequestLogging` / `Settings.Configuration` /
`Sinks.Debug` paketlerini geçişli almaz; ihtiyacı olan kendi referansını ekler.

KEEP (gerçekten `Microsoft.AspNetCore.*` kullanır): `Kck.AspNetCore`, `Kck.Exceptions.AspNetCore`, `Kck.Bundle.WebApi`, `Kck.Bundle.MinimalApi`,
`Kck.Observability.OpenTelemetry` (`MapKckHealthChecks`, `OpenTelemetry.Instrumentation.AspNetCore`). Sonuç: `Kck.Bundle.WorkerService`
kendi kodunda temiz olsa da OpenTelemetry sağlayıcısı üzerinden çerçeveye bağlı kalır — ayrı iş (OTel ASP.NET parçasını ayrı pakete bölme).

Yeni `Microsoft.Extensions.*` kimlikleri (10.0.14): `Caching.Memory`, `Caching.Abstractions`, `Configuration.Abstractions`,
`Configuration.Binder`. `[LoggerMessage]` kaynak üreticisi analyzers klasörüyle gelir ve geçişli akmaz → 11 projede doğrudan
`Microsoft.Extensions.Logging.Abstractions` referansı şart.

## Seçim

- **B**, sürüm **10.0.14** (CPM `Directory.Packages.props`; `Microsoft.Extensions.Configuration.UserSecrets` 10.0.13 satırı
  kullanılmıyor — F5'te silinir veya aynı hatta çekilir).
- Kabul: Ö13 — `dotnet pack Kck.Caching.InMemory` nuspec'inde `frameworkReferences` yok, `Microsoft.Extensions.Caching.Memory`
  bağımlılığı var; `Microsoft.NET.Sdk.Worker` tüketicisi ASP.NET Core çerçevesiz imajda çalışır (AotSmoke / template-smoke kanıtı).
- Kayıt: ADR `docs/adr/0032-*.md` (mimari: paket bağımlılık yüzeyi), CHANGELOG `[Unreleased]` Changed, geçiş rehberi
  `docs/migrations/v4-to-v5.md` ("tüketici bağımlılık grafiği değişir; API değişmez").
- Lock dosyaları: solution-wide `dotnet restore --force-evaluate` → `--locked-mode` (CentralTransitive dersi).
- EOL: .NET 10 — 2028-11-14.
- Pre-commitment hash: bu dosyanın "Pre-commitment" paragrafı (tasks/checkpoint.md "F3" bölümüyle aynı).
