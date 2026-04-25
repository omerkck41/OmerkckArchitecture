# ADR-0011: Multi-Target net8.0 + net10.0

Tarih: 2026-04-26
Durum: Onaylandi

## Baglam

Library Strategy raporu (2026-04-25), Bolum 2.1: kutuphane sadece `net10.0`
hedefliyor. .NET 10 STS (Standard Term Support) — 18 ay destek. Buyuk
kurumsal kullanicilar net8.0 LTS'te. NuGet istatistiklerine gore 2026-04
itibariyle .NET indirilenlerinin %70+'i net8.0 hedefli. Bu durumda kutuphane
kullanici tabani dramatik daraliyor; LTS'i destekleyen rakipler (ABP,
FastEndpoints, Aspire) avantaj kaziniyor.

## Karar

Tum **abstraction** paketleri (`src/Abstractions/*`) ve **saf provider**
paketleri (transitive olarak ASP.NET Core 10 veya EF Core 10 cekmeyenler)
`net8.0;net10.0` multi-target hale getirildi. Net10-only kalanlar:

- `src/Bundles/Kck.Bundle.WebApi` (ASP.NET Core 10)
- `src/Providers/Kck.Persistence.EntityFramework` (EF Core 10 — net8 desteklemiyor, NU1202)
- `src/Providers/Kck.AspNetCore` (ASP.NET Core 10 framework)
- `src/Providers/Kck.Security.Jwt` (Microsoft.AspNetCore.Authentication.JwtBearer 10.0.5)
- `src/Providers/Kck.Caching.Redis` (Microsoft.Extensions.Caching.StackExchangeRedis 10.0.7)
- `src/Providers/Kck.Http.Resilience` (Microsoft.Extensions.Http.Resilience 10.5.0)
- `src/Providers/Kck.Exceptions.AspNetCore` (ASP.NET Core 10)
- `samples/*` (tum sample uygulamalar Bundle.WebApi kullaniyor)
- `tests/**/*` (test cikti ureticisi degil; multi-target test ileride bir faz olarak ele alinacak)

## Konfigurasyon

`Directory.Build.props` icinde:

```xml
<TargetFrameworks Condition=" '$(TargetFrameworks)' == '' ">net8.0;net10.0</TargetFrameworks>
```

Net10-only csproj'lar `<TargetFrameworks>net10.0</TargetFrameworks>` (cogul) ile
override eder. Cogul kullanim onemli — tekil `<TargetFramework>` syntax'inin
Directory.Build.props'in TargetFrameworks default'unu temizlememesi nedeniyle
NU1202 patliyor; cogul ise condition tarafindan dogrudan algilanip override
ediliyor.

Net8'de bulunmayan API'lar icin `#if NET9_0_OR_GREATER` ile fallback eklendi:

| Dosya | Net9+ API | Net8 Fallback |
|---|---|---|
| `IntegrationEvent.cs` | `Guid.CreateVersion7()` | `Guid.NewGuid()` |
| `QuartzJobScheduler.cs` | `Guid.CreateVersion7()` | `Guid.NewGuid()` |
| `TotpMfaProvider.cs` | `Convert.ToHexStringLower(bytes)` | `Convert.ToHexString(bytes).ToLowerInvariant()` |

## Alternatifler Degerlendirildi

1. **net8 + net9 + net10 uclu multi-target:** Reddedildi — net9 STS'i kullanan
   az kullanici var (NuGet stats), ek build suresi haklilastiramaz. .NET 12
   LTS (2027 Kasim) yayinlandiginda net12 eklenir, net9 dustugunde cikarilir.
2. **Sadece net8 LTS hedefi:** Reddedildi — net10 ozel API kazanimlari
   kullanilamaz.
3. **netstandard2.0 abstraction'lar:** Reddedildi — `IAsyncDisposable`,
   `required`, `init`, source-gen attribute'lari netstandard2.0'da yok.
4. **Tum projeleri kapsam B disinda multi-target etmek (ASP.NET Core 10 dahil):**
   Reddedildi — spike'ta EF Core 10 NU1202 verdi, ASP.NET Core paketleri benzer
   sekilde yalnizca net10 hedefli. Zorla net8 surumlerine cekmek major
   downgrade riski (8.0.x serisi feature parity yok).
5. **Directory.Build.targets ile geç-evaluation:** Reddedildi — multi-target
   outer build'de Directory.Build.targets TargetFrameworks set edemiyor;
   MSB3992 hatasiyla patliyor.

## Sonuclar

**Olumlu:**
- Pazarlanabilir kullanici tabani 3-5x genisler (rapor tahmini).
- LTS taban + STS uc avantaji.
- Abstraction paketleri en yaygin kitleyi yakalar.
- Multi-target paketler `lib/net8.0/` + `lib/net10.0/` dual-folder NuGet uretir.

**Olumsuz:**
- Restore/build suresi multi-target nedeniyle ~%30-50 artar (her TFM ayri compile).
- Iki TFM yuzeyi icin xUnit testleri **ileride** ayri kosturulmali (LS-FAZ-4 kapsami).
- Conditional `#if NET9_0_OR_GREATER` kod kosulu net8 fallback yerlerine girdi —
  4 yer; minimum invasive.

## Referanslar

- `tasks/library-strategy-2026-04-25.md` (Bolum 2.1)
- `tasks/library-strategy-faz2-multitarget-2026-04-26.md`
- `tasks/checkpoint.md` (Spike Bulgulari)
- [.NET Support Policy](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)
- [EF Core 10 release notes](https://learn.microsoft.com/ef/core/what-is-new/ef-core-10.0/whatsnew)
