# Surum Politikasi (SemVer)

Bu dokuman OmerkckArchitecture (`Kck.*`) paketlerinin surum numaralandirma
ve breaking change politikasini tanimlar.

## Genel Kural — [SemVer 2.0.0](https://semver.org/)

`MAJOR.MINOR.PATCH`

| Surum bumpi | Anlami |
|---|---|
| MAJOR | Public API'da breaking change (kaldirilan / imzasi degisen API) |
| MINOR | Public API'a additive (yeni tip, yeni metot, yeni ozellik) |
| PATCH | Bug fix, internal refactor, dokuman, performans iyilestirmesi |

Kutuphane `MinVer` ile git tag uzerinden otomatik versiyonluyor — tag yapisi: `vX.Y.Z`.

## "Public API" Tanimi

Asagidakilerin hepsi public API kapsamindadir:

- `public` modifier ile isaretli ve `Kck.*` namespace altinda yer alan **tum** tipler ve uyeler:
  - sinif, struct, record, interface, enum, delegate
  - metot, property, field, event, constructor
  - generic constraint, default interface implementation
- `protected` ve `protected internal` uyeler **public sayilir** (turevin gormesi yeterli).
- `internal` uyeler public sayilmaz (ancak `[InternalsVisibleTo]` ile actigin disardaki paket icin tasarim borcu olabilir — kacin).

## Breaking Change Tanimi

Asagidaki herhangi bir degisiklik **breaking** sayilir:

- Public tipi/uyeyi kaldirma
- Public uye imzasini degistirme (parametre tipi, sayisi, donus tipi)
- Default deger kaldirma / degistirme
- Generic constraint ekleme/sertleştirme
- Inheritance hiyerarsisini degistirme (base class swap)
- Erisim seviyesini daraltma (`public` → `internal`, `protected` → `private`)
- Davranissal sozlesme degisikligi (ornegin `null` donen bir API artik exception atiyor)

## Politika

### 0.x Donemi (mevcut)

- Minor bump'larda **breaking olabilir**, ancak:
  - `CHANGELOG.md` `### Breaking` altinda **acik** belirtilir.
  - Mumkun olan yerde once `[Obsolete]` ile uyari verilir
    ([deprecation policy](deprecation.md) gerei).
  - `PublicAPI.Shipped.txt` dosyasi guncellenir (PublicApiAnalyzers).

### 1.0 Sonrasi (planlanan)

- **MINOR sadece additive olabilir.**
- Breaking change sadece **MAJOR**'da yapilir.
- Breaking once `[Obsolete]` ile en az **iki minor** boyunca uyari verir
  (deprecation policy: N+0.x ve N+1.x).
- Breaking change'ler `docs/migrations/v(N-1).x-to-vN.0.md` dokumanina yazilir.

## PublicApiAnalyzers ile Tracking

Tum `src/Abstractions/*` projeleri `Microsoft.CodeAnalysis.PublicApiAnalyzers`
kullanir. Her abstraction projesinde:

- `PublicAPI.Shipped.txt` — sayilan tum mevcut public API
- `PublicAPI.Unshipped.txt` — bir sonraki release'de eklenecek/silinecek API

Yeni public API eklendiginde build hatasi (RS0016) verilir. PR'da
`PublicAPI.Unshipped.txt`'a satir eklenmesi zorunludur.

Release sirasinda `PublicAPI.Unshipped.txt` icerigi `PublicAPI.Shipped.txt`
dosyasina tasinir, `Unshipped` bos kalir.

## CHANGELOG Disiplini

Her release oncesi `CHANGELOG.md`'nin `[Unreleased]` bolumu su 5 baslik
altinda toplanir ([Keep a Changelog](https://keepachangelog.com/en/1.1.0/)):

- `### Added` (yeni ozellik / API)
- `### Changed` (mevcut davranis degisikligi)
- `### Deprecated` (gelecek major'da kalkacak)
- `### Removed` (artik var olmayan)
- `### Fixed` (bug fix)
- `### Security` (guvenlik fix'i)

`### Removed` veya `### Changed` icindeki breaking degisiklikler
**MAJOR bump** gerektirir (1.0+ sonrasi).

## Yeni Public API Eklerken

1. Kodu yaz, `dotnet build` cikis verir (RS0016).
2. `PublicAPI.Unshipped.txt`'a uretilen satiri kopyala.
3. XML doc yaz (eksik docs faz'i sonrasi: zorunlu olacak).
4. `CHANGELOG.md` `### Added` altina kisa aciklama ekle.

## Public API Kaldirirken / Degisirken

1. Once `[Obsolete]` isaretle (DiagnosticId zorunlu — [deprecation.md](deprecation.md)).
2. `PublicAPI.Unshipped.txt`'a `*REMOVED*` satiri ekle (PublicApiAnalyzers konvansiyonu).
3. `CHANGELOG.md` `### Deprecated` altina yaz.
4. En az iki minor sonra major bump'ta kaldir.

## Referanslar

- [SemVer 2.0.0](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
- [Roslyn PublicApiAnalyzers](https://github.com/dotnet/roslyn-analyzers/tree/main/src/PublicApiAnalyzers)
- [.NET Library Guidance: Breaking Changes](https://learn.microsoft.com/dotnet/standard/library-guidance/breaking-changes)
- [`docs/policies/deprecation.md`](deprecation.md)
- [`docs/policies/support.md`](support.md)
