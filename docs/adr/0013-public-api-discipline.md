# ADR-0013: Public API Disiplini (PublicApiAnalyzers + SemVer + Obsolete)

Tarih: 2026-04-26
Durum: Onaylandi

## Baglam

Library Strategy raporu (2026-04-25) Bolum 4.7, 4.8 ve 12.3:

- Kutuphane `MinVer` ile git tag tabanli otomatik versiyonluyor (mukemmel) ancak
  hangi degisikligin breaking oldugu **otomatik tespit edilmiyor**.
- `PublicAPI.txt` (PublicApiAnalyzers tracking dosyasi) **yok**; her release'de
  yanlislikla breaking change yapma riski yuksek.
- "Public API" tanimi tanimsiz; Obsolete + DiagnosticId disiplini ADR-0010 ile
  tanimli ama public surface tracking eksik.

Roslyn ve EF Core gibi referans projeler `Microsoft.CodeAnalysis.PublicApiAnalyzers`
kullaniyor: `PublicAPI.Shipped.txt` + `PublicAPI.Unshipped.txt` ikilisi her
PR'da public surface degisikligini gozler onune seriyor.

## Karar

`Microsoft.CodeAnalysis.PublicApiAnalyzers` paketi **tum
`src/Abstractions/*` projelerine** (16 paket) eklenir. Her abstraction
projesinde:

- `PublicAPI.Shipped.txt` — mevcut public API baseline (build hata vermesin diye doldurulur)
- `PublicAPI.Unshipped.txt` — bos baslar; yeni eklemeler buraya yazilir

Yeni public API → analyzer **RS0016** ("Add public types and members to the
declared API") uyarisi → `Unshipped.txt`'a satir eklenmedikce build kirilir.

`docs/policies/versioning.md` tum public API tanimini, breaking change
politikasini ve PublicApiAnalyzers ile tracking surecini netlestirir.

## Provider'lar Neden Disinda?

Provider paketleri `Kck.<Domain>.<Tech>` (ornek: `Kck.Caching.Redis`) public
surface'i:

- Cogunlukla `IServiceCollection` extension'lari (`AddKckCachingRedis`)
- Domain-spesifik options sinifi (`KckCachingRedisOptions`)
- Implementasyon sinifi (cogunlukla internal yapilmali ama henuz public)

Provider'larda public surface daha **akiskan** (rename/refactor sik), abstraction
kontrati **kararli** olmali. Bu nedenle:

- Asama 1 (bu ADR): sadece abstraction'lar tracking altina alinir.
- Asama 2 (sonraki faz): provider'larda gercekten public olmasi gereken
  surface ayrilir, internal yapilabilen tipler `internal` isaretlenir, sonra
  PublicApiAnalyzers eklenir.

Bu staged yaklasim "internal sizinti" sorununu (Bolum 4.7) cozmek icin yeterli
zamani kazandirir.

## Alternatifler Degerlendirildi

1. **`Microsoft.DotNet.ApiCompat`:** Reddedildi — package-to-package compat
   icin tasarlanmis (NuGet pack sonrasi), kaynak duzeyinde PR feedback
   vermiyor. PublicApiAnalyzers kaynak duzeyinde IDE'de uyari verir.

2. **Manuel `Diff.txt` review:** Reddedildi — disiplin gerekiyor, otomatize
   edilebilir bir is hatayla daha guvenilir.

3. **Sadece major release oncesi audit:** Reddedildi — 0.x doneminde her minor
   breaking olabildigi icin gec geri-bildirim yikici.

4. **PublicApiAnalyzers tum projelerde:** Reddedildi (su an icin) — provider
   surface temizligi yapilmadan baseline olusturmak teknik borc kilitleyici.

## Sonuclar

**Olumlu:**
- Yeni public API ekleme PR'da goruluyor (RS0016).
- `Unshipped.txt` review'da diff goruyor — breaking change yanlislikla
  yapilamaz.
- Roslyn ve EF Core gibi referans kutuphanelerle uyumlu.
- `docs/policies/versioning.md` ile public API tanimi netlesir.

**Olumsuz:**
- 16 abstraction projesinde baseline yazma efforu (~1500-2500 satir).
- PR yazaninin `Unshipped.txt`'i guncel tutmasi gerekiyor (CONTRIBUTING.md
  gunceleneek faz icindeki commit'le).
- Provider'lar bu ADR kapsami disinda — ayri faz gerekir.

## Konfigurasyon

Her abstraction csproj'una:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.CodeAnalysis.PublicApiAnalyzers" PrivateAssets="all" />
  <AdditionalFiles Include="PublicAPI.Shipped.txt" />
  <AdditionalFiles Include="PublicAPI.Unshipped.txt" />
</ItemGroup>
```

Paket `Directory.Packages.props` ile tek surumde tutulur (CPM aktif zaten).

## Referanslar

- `tasks/library-strategy-2026-04-25.md` Bolum 4.7, 4.8, 12.3
- `tasks/library-strategy-faz3-public-api-2026-04-26.md`
- [Roslyn PublicApiAnalyzers](https://github.com/dotnet/roslyn-analyzers/tree/main/src/PublicApiAnalyzers)
- [Roslyn PublicAPI.txt format](https://github.com/dotnet/roslyn-analyzers/blob/main/src/PublicApiAnalyzers/PublicApiAnalyzers.Help.md)
- [.NET Library Guidance: API Stability](https://learn.microsoft.com/dotnet/standard/library-guidance/breaking-changes)
- ADR-0010 (Deprecation Policy)
- `docs/policies/versioning.md`, `docs/policies/support.md`
