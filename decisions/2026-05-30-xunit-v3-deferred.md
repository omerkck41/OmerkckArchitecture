# Karar: xunit v3 migration ERTELENDİ (Stryker.NET uyumsuzluğu)

- **Tarih:** 2026-05-30
- **Durum:** NO-GO (değerlendirildi, reddedildi — ileride yeniden değerlendirilecek)
- **Bağlam:** `dotnet list package --deprecated` xunit 2.9.3'ü "Legacy" (deprecated, yerine
  xunit.v3) gösterdi. v3'e geçiş değerlendirildi: premortem + tek-proje pilot.

## Pilot bulguları (Kck.Core.Abstractions.Tests, VSTest-uyumlu mod)
- ✅ `xunit.v3` 3.2.2 + `OutputType=Exe` + runner 3.1.5 → build + 65 test geçti
- ✅ `dotnet test` coverage collection (XPlat/coverlet) → cobertura üretildi (VSTest EXE'yi keşfetti)
- ✅ CsCheck (property-based) v3 ile çalıştı
- ✅ Kaynak breaking change YOK (repo'da 0 `Xunit.Abstractions`, 0 `async void`, 0 `ITestOutputHelper`)
- ❌ **Stryker.NET 4.14.2 + xunit.v3 = mutation skoru %0** (58 mutant, 0 killed/0 survived,
  "Final mutation score is below threshold break. Crashing...")

## Blocker
Stryker.NET 4.x, xunit.v3 testlerini mutant başına çalıştıramıyor — **bilinen, çözülmemiş** sorun:
- https://github.com/stryker-mutator/stryker-net/issues/3117 (erişim: 2026-05-30) — "Stryker.NET doesn't handle xUnit v3 properly"
- https://github.com/stryker-mutator/stryker-net/issues/3094 (erişim: 2026-05-30) — MTP runner desteği (açık)
Kök neden: xunit.v3'ün Microsoft Testing Platform mimarisi; Stryker henüz MTP runner desteklemiyor.
**Sürüm bump'ı çözmez** (issue açık).

## Gerekçe (NO-GO)
Bu kod tabanında **7 Stryker mutation config** kritik kalite altyapısı (core/caching/security/
totp/localization/jwt/pipeline — bu oturumda %80–95 skorlara çıkarıldı). v3'e geçiş hepsini
%0'a düşürür ve mutation CI workflow'unu kırar. Kazanç (deprecated-ama-bakımlı xunit v2'den
kurtulmak) << maliyet (tüm mutation testing kaybı). xunit v2.9.3 hâlâ güvenlik fix'i alıyor.

## Yeniden değerlendirme tetikleyicisi
Stryker.NET MTP/xunit.v3 desteğini yayınladığında (issue #3094/#3117 kapandığında) pilotu tekrarla.
O zamana kadar xunit v2.9.3'te kal.

- **EOL:** xunit v2 — resmi "yalnız güvenlik fix" modunda; sabit EOL tarihi ilan edilmedi (N/A).
- **Pre-commitment:** Premortem'de Stryker uyumsuzluğu "Orta olasılık" risk olarak öngörüldü;
  pilot bunu doğruladı (rasyonalizasyon değil, önceden beyan edilen riskin teyidi).
