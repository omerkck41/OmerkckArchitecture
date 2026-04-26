# Migration vX.x → vY.0

**Yayin tarihi:** YYYY-MM-DD
**Etki:** Breaking
**Tahmini effort:** S / M / L (kullanicinin kod buyuklugune gore)

## Neden Bu Major?

Bu major'in sebebini 1-2 paragrafta acikla. Hangi mimari karar (ADR-XXXX)
tetikledi? Gerekce.

## Etkilenen Paketler

| Paket | Degisiklik Tipi | Etki Seviyesi |
|---|---|---|
| `Kck.Core.Abstractions` | API rename | Orta |
| `Kck.Caching.Redis` | Servis kayit imzasi | Yuksek |

(Etki: Dusuk = sadece interface implementasyonlari, Orta = kullanim siteleri,
Yuksek = configuration / DI seviyesi)

## Hizli Bakis (TL;DR)

- Degisiklik 1 (3-5 kelime ile)
- Degisiklik 2
- Degisiklik 3

## Detayli Degisiklikler

### 1. `<API adi>` Kaldirildi/Yeniden Adlandirildi

**Sebep:** ...

**Once (vX):**
```csharp
services.AddKckXxx(opt => opt.Foo = "bar");
```

**Sonra (vY):**
```csharp
services.AddKckXxx(opt => opt.Bar = "foo");
```

**DiagnosticId:** `KCKnnnn` (varsa)

### 2. `<API adi>` Davranis Degisikligi

...

## Otomatik Migration

Mumkun olan donusumler:

- **Roslynator analyzer:** `<analyzer-id>` — `dotnet format analyzers`
  ile uygulanabilir.
- **`dotnet format`:** ...
- **IDE quick fix:** Visual Studio / Rider Ctrl+. / Alt+Enter — Obsolete
  uyarisi tikla, "Use suggested replacement".

## Manuel Adimlar

Otomatiklestirilemeyen yerler:

1. Configuration (`appsettings.json`) anahtar yeniden adlandirmasi: `Foo` → `Bar`.
2. ...

## Referans

- ADR-XXXX
- CHANGELOG.md `[vY.0.0]`
- `docs/policies/versioning.md`
