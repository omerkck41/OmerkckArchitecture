# Destek Politikasi (.NET Surum Matrisi)

Bu dokuman OmerkckArchitecture (`Kck.*`) paketlerinin hedef framework
destek soyutlamasini tanimlar.

## Genel Kural

Kutuphane Microsoft'un .NET destek politikasini takip eder:

- **LTS** (Long Term Support, 3 yil): kutuphane **butun yasam dongusu boyunca** destekler.
- **STS** (Standard Term Support, 18 ay): kutuphane **destek penceresi suresince** hedefler.
- LTS donemi sonu yaklastiginda yeni LTS yayinlandiktan sonra eski LTS kademeli olarak kaldirilir.

## Mevcut Destek Matrisi (2026-05-04 itibariyle)

| TFM | Tip | Microsoft Destek Sonu | Kutuphane Durumu |
|---|---|---|---|
| `net8.0` | LTS | 2026-11-10 | **Kaldırıldı** (ADR-0019, v2.0) |
| `net10.0` | STS (LTS adayi) | 2027-11-09 | **Birincil hedef** |

> .NET 10 yayin doneminde STS olarak duyurulmus, ancak Microsoft destek sayfasi
> guncellenirse LTS'e cevrilebilir. Resmi durum icin:
> https://dotnet.microsoft.com/platform/support/policy/dotnet-core

## Hedef Framework Karari

**Hedef framework politikasi (ADR-0019):**

- Tüm paketler: `net10.0` (tek hedef)
- ADR-0011 (multi-target net8+net10) ADR-0019 ile yerine geçildi.

Detaylar: [`docs/adr/0019-drop-net8-target.md`](../adr/0019-drop-net8-target.md)

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
- [ADR-0011: Multi-Target net8.0 + net10.0 (Superseded)](../adr/0011-multi-target-net8-net10.md)
- [ADR-0019: net8.0 desteğinin kaldırılması](../adr/0019-drop-net8-target.md)
- `tasks/library-strategy-2026-04-25.md` Bolum 6.4
