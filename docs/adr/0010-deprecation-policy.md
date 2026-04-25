# ADR-0010: Deprecation Policy

Tarih: 2026-04-25
Durum: Onaylandi

## Baglam

Library Strategy raporu (2026-04-25), Bolum 6.2: `[Obsolete]` kullanimi
proje icinde basliyor (KCK0001) ancak (a) DiagnosticId numaralandirma
sistemi tanimsiz, (b) deprecation periyodu yazili politika seklinde
yok, (c) UrlFormat eksik. Pre-1.0 evrede bu eksiklik kucuk; ancak v1.0
freeze'inden once yazili politika gerekli.

## Karar

`docs/policies/deprecation.md` icine resmi politika yazildi:
- En az iki minor surum kaynakta kalma garantisi
- Kategoriye gore DiagnosticId araligi (KCK0001-0999)
- `DiagnosticId` + `UrlFormat` zorunlu
- Deprecation registry tablosu

## Alternatifler Degerlendirildi

1. **Sadece CHANGELOG'a deprecation notu**: Reddedildi — IDE entegrasyonu
   ve search yok, kullanici ne zaman silinecegini bilmiyor.
2. **Major bump'ta sertce sil**: Reddedildi — kullaniciya migration suresi
   tanimiyor; SemVer'in yumusak deprecation tradition'ina aykiri.
3. **Roslyn analyzer ile zorla**: Ileride (LS-FAZ-3) `PublicApiAnalyzers`
   ile yapilacak; bu ADR sadece policy'i belgeleyip otomasyona zemin
   hazirliyor.

## Sonuclar

**Olumlu:**
- Kullanici `[Obsolete]` attribute'unun URL'sine tiklayarak migration
  yolunu kolayca bulabilir.
- DiagnosticId araligi yeni katki sirasinda numaralandirma karisikligini
  onler.
- v1.0 stability anchor icin temel atildi.

**Olumsuz:**
- Yeni `[Obsolete]` ekleyen developer'lar bu dosyaya tablo satiri ek-
  lemek zorunda — manuel kontrol gerekiyor (LS-FAZ-3'te otomasyon).
- `UrlFormat` icindeki anchor (`#{0}`) henuz markdown anchor olarak
  yok; ileride deprecation.md icinde DiagnosticId per anchor eklenmeli.

## Referanslar

- `docs/policies/deprecation.md`
- `tasks/library-strategy-2026-04-25.md` (Bolum 6.2, 12.4)
- [.NET ObsoleteAttribute docs](https://learn.microsoft.com/dotnet/api/system.obsoleteattribute)
