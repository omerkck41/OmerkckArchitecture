# Deprecation Politikasi

Bu dokuman OmerkckArchitecture (Kck.\*) public API'larinin deprecation
yasam dongusunu tanimlar.

## Genel Kurallar

1. **Public API**: `public` modifier ile isaretli ve namespace'i `Kck.*`
   altinda olan tum tipler, metotlar, propertyler, fieldlar, eventler,
   delegateler ve interface uyeleri.
2. **Deprecation periyodu**: Bir public uye `[Obsolete]` ile isaretlendigi
   andan itibaren **en az iki minor surum** (N+0.x ve N+1.x) boyunca
   kaynakta kalmali; major bump'ta (N → N+1.0) kaldirilabilir.
3. **DiagnosticId zorunlu**: Her `[Obsolete]` attribute'unda bir
   `DiagnosticId` ve `UrlFormat` bulunmalidir.

## DiagnosticId Numaralandirmasi

| Prefix         | Kategori                          |
| -------------- | --------------------------------- |
| `KCK0001-0099` | Genel API                         |
| `KCK0100-0199` | Persistence                       |
| `KCK0200-0299` | Caching                           |
| `KCK0300-0399` | Security/Auth                     |
| `KCK0400-0499` | Pipeline (MediatR/Mediator)       |
| `KCK0500-0599` | Observability                     |
| `KCK0600-0699` | Messaging/Email                   |
| `KCK0700-0799` | Background Jobs                   |
| `KCK0800-0899` | EventBus                          |
| `KCK0900-0999` | Localization/Documents/Other      |

## Attribute Sablonu

```csharp
[Obsolete(
    "Aciklama: NEDEN obsolete + ALTERNATIF API. " +
    "Removed in vX.Y.",
    DiagnosticId = "KCKnnnn",
    UrlFormat = "https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/policies/deprecation.md#{0}")]
```

## Yasam Dongusu

```
v0.X.0 — API tanitilir
   ...
v0.Y.0 — API @[Obsolete] ile isaretlenir (DiagnosticId verilir)
v0.Y+1.0 — API hala kaynakta (deprecation period 1)
v1.0.0 — API kaldirilabilir (major bump)
```

## Mevcut Deprecated API Listesi

| DiagnosticId | API                                              | Isaretlendi | Kaldirilacak |
| ------------ | ------------------------------------------------ | ----------- | ------------ |
| KCK0001      | `JwtTokenService.GetClaimsFromToken(string)`     | 0.x         | v1.0         |

## Yeni Deprecation Eklerken

1. `[Obsolete]` attribute'unu sablona uygun ekle.
2. Bu dosyaya tablo satiri ekle.
3. CHANGELOG.md'nin `[Unreleased]` bolumune `### Deprecated` altina yaz.
4. ADR-0010 (deprecation policy) ile celisme yoksa onayli kabul et.

## Referans

- [.NET Deprecation guidance](https://learn.microsoft.com/dotnet/standard/library-guidance/breaking-changes)
- [Roslyn ObsoleteAttribute DiagnosticId](https://learn.microsoft.com/dotnet/api/system.obsoleteattribute.diagnosticid)
