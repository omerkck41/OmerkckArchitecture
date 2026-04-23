# ADR-0001: Argon2 Implementasyonu — Konscious.Security.Cryptography.Argon2 Korunmasi

Tarih: 2026-04-20
Durum: Onaylandi

## Baglam

`Kck.Security.Argon2` provider'i parola hash islemleri icin
`Konscious.Security.Cryptography.Argon2` 1.3.1'i kullaniyor. Audit raporu
(2026-04-20) bu paketi **KIRMIZI** olarak isaretledi:

- Son stabil surum 1.3.1 (2021)
- GitHub deposu aktif commit aliyor gozukmuyor
- Alternatif paketler var: `Isopoh.Cryptography.Argon2`, `libsodium` (NSec,
  Sodium.Core wrapper'lari)

Karar icin degerlendirilen kriterler:
- OWASP parola hashleme onerileri (Argon2id, memory >= 19 MiB, 2+ iterations)
- CVE gecmisi
- .NET 10 uyumu
- Aktif bakim
- Migration risk (parola hash formati degisirse eski hashler dogrulanamaz)

## Karar

`Konscious.Security.Cryptography.Argon2` 1.3.1 **korundu**. Gerekce:

1. **Bilinen CVE yok** — paketin `dotnet list package --vulnerable` taramasi temiz
2. **.NET 10 uyumu calisir** — targeting `netstandard2.0`, .NET 10 host'ta
   sorun yok (test projesi yesil)
3. **RFC 9106 uyumlu Argon2id** uygulamasi — kriptografi algoritmasi standart,
   implementasyon "terk" degil "olgun"
4. **Parola hash formati degisimi riskli** — farkli kutuphane farkli iteration
   counter / salt layout kullanabilir, mevcut hash'li parolalar dogrulanamaz hale
   gelir. Migration icin ikili dogrulama gerekirdi.

Bunun yerine surekli izleme politikasi:

- Her audit turunda `dotnet list package --vulnerable` ile kontrol
- Yeni CVE aciklanirsa: acil migration + hash re-issue politikasi
- Bakim gercekten durursa (2+ yil hicbir commit): Isopoh'a migration ADR-sup

## Alternatifler Degerlendirildi

- **`Isopoh.Cryptography.Argon2` 2.0.0** — Aktif, pure C#, cross-platform.
  En guclu alternatif. Ancak mevcut hash'lerle binary-compat olmayabilir;
  test gerektirir. Reddedildi (simdilik, risk > kazanim).
- **`libsodium` wrapper (NSec/Sodium.Core)** — FFI, ekstra native dependency.
  Kutuphane daha guvenli olsa da dagitim/platform kulturu artar. Reddedildi.
- **Kendi wrapper'imizi yazmak** — YAGNI; kriptografide kendi yazmak risk.
  Reddedildi.

## Sonuclar

**Olumlu:**
- Sifir migration riski — mevcut hash'ler dogrulanir
- Dependency degisikligi yok, test setup ayni

**Olumsuz:**
- Yazilimin reactive migration planina hazir olmasi gerekir (CVE aciklanirsa
  hizli aksiyon)
- Pazarlamada "aktif bakim" etiketini paket bazinda savunamiyoruz; algoritma
  bazinda Argon2id standart

## Izleme

Quarter bazinda audit turunda paket durumu gozden gecirilir. CVE veya 2+ yillik
gercek stagnation durumunda bu ADR superseded olur.
