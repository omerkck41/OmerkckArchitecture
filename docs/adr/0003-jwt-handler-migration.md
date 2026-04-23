# ADR-0003: JWT Handler Migration — JwtSecurityTokenHandler → JsonWebTokenHandler

Tarih: 2026-04-20
Durum: Onaylandi

## Baglam

`Kck.Security.Jwt` provider'i `System.IdentityModel.Tokens.Jwt` 8.17.0 icindeki
`JwtSecurityTokenHandler` tipini kullaniyordu. Microsoft bu handler'in yerine
`Microsoft.IdentityModel.JsonWebTokens` paketindeki `JsonWebTokenHandler`'i
oneriyor:

- `JsonWebTokenHandler` ~2-3x daha hizli (span-based parsing, daha az allocation)
- Legacy `ClaimsPrincipal` sisteminden `Claim[]`/`JsonWebToken` uzerine gecis
- `TokenValidationResult` zaten `handler.ValidateTokenAsync` donusu — API neredeyse ayni
- `System.IdentityModel.Tokens.Jwt` bakim modunda; yeni ozellikler yalniz
  JsonWebTokens paketine geliyor

Audit raporu (2026-04-20) bu migrasyonu SARI (modern) olarak isaretledi.

## Karar

`System.IdentityModel.Tokens.Jwt` referansi `Microsoft.IdentityModel.JsonWebTokens`
ile degistirildi. Her iki paket de ayni surum (8.17.0) kullaniyor, uyum sorunu yok.

Kod tarafi:
- `JwtSecurityToken(...) + new JwtSecurityTokenHandler().WriteToken(token)` →
  `new JsonWebTokenHandler().CreateToken(SecurityTokenDescriptor)`
- `handler.ValidateTokenAsync(token, validationParams)` — imza ve donus tipleri ayni
- `handler.ReadJwtToken(token)` → `new JsonWebToken(token)` — claim yapisi ayni

Public API (`ITokenService`) imzalari degismedi; test davranisi ayni.

## Alternatifler Degerlendirildi

- **Mevcut handler'i koru:** Performans kaybi + modern ekosistemden kopus. Reddedildi.
- **Her ikisini tut:** Gereksiz bagimlilik. Reddedildi.
- **IdentityModel v9'a atla:** Major surum, breaking changes var. 8.17.0 stabil,
  su an icin yeterli. Gelecek iterasyonda degerlendirilebilir.

## Sonuclar

**Olumlu:**
- Tipik token uretim/dogrulama ~2-3x hizlandi
- Microsoft resmi onerisiyle hizali
- Daha az heap allocation — GC baskisi dustu

**Olumsuz:**
- Public surface'ta breaking yok ama internal kullanim `SecurityTokenDescriptor`
  API'sine gore yeniden yazildi — gelecekte bu tip degisirse dokunmak gerekir

## Dogrulama

`JwtTokenServiceTests` (6 test) — tamamen gecti, davranis degismedi.
