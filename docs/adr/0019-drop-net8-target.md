# ADR-0019: net8.0 Desteğinin Kaldırılması

Tarih: 2026-05-04
Durum: Onaylandi
Supersedes: ADR-0011 (Multi-target net8.0 + net10.0)

## Bağlam

ADR-0011 (2026-04-26), kullanıcı tabanını genişletmek amacıyla
`src/Abstractions/*` ve saf provider'ları `net8.0;net10.0` multi-target
hale getirdi. Kararın gerekçesi buyuk kurumsal kullanicilarin net8.0 LTS
tercihiydi.

Değerlendirme (2026-05-04): Proje, .NET 10 ve üzeri modern API'ları (AOT,
`Guid.CreateVersion7()`, `Convert.ToHexStringLower()`) birincil tasarım
kararı olarak benimsiyor. net8.0 desteği bu modern API'ları `#if` bloklarıyla
kısıtlıyor, build süresini ~%30-50 artırıyor ve kütüphanenin ".NET 10 ve
üzeri" konumlandırmasıyla çelişiyor.

## Karar

`Directory.Build.props` içindeki default TFM `net8.0;net10.0`'dan `net10.0`'a
değiştirildi. Tüm projeler artık yalnızca `net10.0` hedefler.

Eş zamanlı temizlik:
- `IntegrationEvent.cs` — `#if NET9_0_OR_GREATER / #else Guid.NewGuid()` kaldırıldı
- `QuartzJobScheduler.cs` — `#if NET9_0_OR_GREATER / #else Guid.NewGuid()` kaldırıldı
- `TotpMfaProvider.cs` — `#if NET9_0_OR_GREATER / #else Convert.ToHexString()` kaldırıldı

## Gerekçe

1. **Tasarım tutarlılığı:** Kütüphane ".NET 10+ first" olarak konumlandırıldı;
   net8 multi-target bu mesajla çelişiyor.
2. **Kod sadeleşmesi:** `#if NET9_0_OR_GREATER` blokları temizleniyor —
   her TFM için ayrı build yok.
3. **Build süresi:** ~%30-50 derleme süresi azalması.
4. **net8.0 EOS:** .NET 8.0 destek sonu 2026-11-10 — 6 ay içinde EOL.
5. **Paket uyumu:** EF Core 10, ASP.NET Core 10 gibi bağımlılıklar
   zaten net8.0'ı desteklemiyor; multi-target bu projelerde zaten anlamsızdı.

## Alternatifler Değerlendirildi

1. **net8.0 desteğini korumak:** Reddedildi — 2026-11-10 EOS yakın,
   kod karmaşıklığı artıyor, modern API kullanımı kısıtlanıyor.
2. **net8.0 + net10.0 + net12.0 planlamak:** Reddedildi — net8 EOS sonrası
   kaldırma işlemi yine gerekecek; erken kaldırmak daha temiz.

## Sonuçlar

**Olumlu:**
- Kod tabanı sadeleşiyor (3 `#if` bloğu kaldırıldı)
- Build süresi azalıyor
- NuGet paketi yalnızca `lib/net10.0/` içeriyor — daha sade

**Olumsuz:**
- net8.0 kullanan tüketiciler kütüphaneyi kullanamaz (breaking change)
- Etki: v2.0 major bump gerektirir (SemVer uyumu için)

## Referanslar

- ADR-0011: Multi-target net8.0 + net10.0 (Superseded)
- [.NET Support Policy — .NET 8 EOS 2026-11-10](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)
