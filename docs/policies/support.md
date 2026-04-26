# Destek Politikasi (.NET Surum Matrisi)

Bu dokuman OmerkckArchitecture (`Kck.*`) paketlerinin hedef framework
destek soyutlamasini tanimlar.

## Genel Kural

Kutuphane Microsoft'un .NET destek politikasini takip eder:

- **LTS** (Long Term Support, 3 yil): kutuphane **butun yasam dongusu boyunca** destekler.
- **STS** (Standard Term Support, 18 ay): kutuphane **destek penceresi suresince** hedefler.
- LTS donemi sonu yaklastiginda yeni LTS yayinlandiktan sonra eski LTS kademeli olarak kaldirilir.

## Mevcut Destek Matrisi (2026-04-26 itibariyle)

| TFM | Tip | Microsoft Destek Sonu | Kutuphane Durumu |
|---|---|---|---|
| `net8.0` | LTS | 2026-11-10 | **Destekli** (LS-FAZ-2 sonrasi) |
| `net10.0` | STS (LTS adayi) | 2027-11-09 (STS olarak), kontroI:Microsoft sayfasi | **Birincil hedef** |

> .NET 10 yayin doneminde STS olarak duyurulmus, ancak Microsoft destek sayfasi
> guncellenirse LTS'e cevrilebilir. Resmi durum icin:
> https://dotnet.microsoft.com/platform/support/policy/dotnet-core

## Hedef Framework Karari

**Multi-target politikasi (ADR-0011):**

- `src/Abstractions/*`: `net8.0;net10.0` (16 paket — multi-target)
- Saf provider'lar (transitive olarak ASP.NET Core 10 / EF Core 10 cekmeyen): `net8.0;net10.0`
- Net10-only provider'lar: ASP.NET Core 10 veya EF Core 10 cekenler — tek hedef `net10.0`

Multi-target detaylari: [`docs/adr/0011-multi-target-net8-net10.md`](../adr/0011-multi-target-net8-net10.md)

## Surum Cikarma Politikasi

- LTS sonu: en az **bir minor** boyunca uyari donemi (CHANGELOG `### Deprecated` → `Target framework X removal`).
- Major bump (1.0 sonrasi): TFM kaldirma sadece major'da.
- 0.x doneminde: minor bump'larda TFM degisikligi yapilabilir (CHANGELOG ile).

## Yeni TFM Eklerken / Kaldirirken

1. ADR yaz (Bolum 11.x) — gerekce + alternatifler.
2. `Directory.Build.props` icindeki default `<TargetFrameworks>` guncelle.
3. Net10-only csproj'lari gozden gecir (multi-target'a aday var mi?).
4. CI matrix (`.github/workflows/build-test.yml`) yeni TFM'yi build/test eder.
5. `CHANGELOG.md` `[Unreleased]` altina kayit dus.
6. Bu dokumandaki matrisi guncelle.

## Referanslar

- [.NET Support Policy (Microsoft)](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)
- [ADR-0011: Multi-Target net8.0 + net10.0](../adr/0011-multi-target-net8-net10.md)
- `tasks/library-strategy-2026-04-25.md` Bolum 6.4
