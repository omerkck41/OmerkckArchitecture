# ADR-0012: Remove AutoMapper Package

Tarih: 2026-04-26
Durum: Onaylandi

## Baglam

Library Strategy raporu (2026-04-25), Bolum 2.3: `Directory.Packages.props`
icinde `AutoMapper 16.1.1` PackageVersion girisi mevcut, ancak src/, samples/,
tests/ icinde **sifir kullanim** tespit edildi (grep dogruladi). AutoMapper
13.x'ten beri ucretli/komersiyel lisansa dogru evrilen bir paket; kullanilmayan
bir paket'in hem central package management listesinde durmasi hem de bagimlilik
guvenlik yuzeyinde olmasi gereksiz risk.

## Karar

`AutoMapper` PackageVersion girisini `Directory.Packages.props`'tan kaldir.

## Alternatifler Degerlendirildi

1. **Birakmak:** Reddedildi — kullanilmayan paket version pin gerektiriyor,
   audit gurultusu yaratiyor.
2. **Sample'a tasimak:** Reddedildi — sample'larda da kullanilmiyor; "ileride
   gerekirse" YAGNI.
3. **Mapster ile degistirmek:** Reddedildi — kutuphane mapping API'si sunmuyor;
   bunu opsiyonel `Kck.Mapping.Mapster` provider'i olarak ileride degerlendir
   (LS-FAZ-7 veya sonraki).

## Sonuclar

**Olumlu:**
- Bagimlilik yuzeyi azalir (transitive dahil).
- AutoMapper'in lisans degisikligi riski sifira iner.
- License-audit workflow'unun tarayacagi paket sayisi azalir (FAZ-1 #16).

**Olumsuz:**
- Yok — sifir kullanim teyit edildi.

## Referanslar

- `tasks/library-strategy-2026-04-25.md` (Bolum 2.3)
- AutoMapper 16 lisans tartismasi: https://github.com/AutoMapper/AutoMapper/issues
