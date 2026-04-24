# Localization

Cok dilli kaynak yonetimi. `ILocalizationService` cultured string lookup,
pluralization ve parametre interpolation saglar.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.Localization.Abstractions` | `ILocalizationService`, `IResourceProvider`, `IPluralizer`, `IFormatterService` |
| `Kck.Localization` | Core service + default pluralizer/formatter |
| `Kck.Localization.Json` | JSON dosya kaynak saglayici |
| `Kck.Localization.Yaml` | YAML dosya kaynak saglayici |

## JSON Setup

```csharp
services.AddKckLocalization(opt =>
{
    opt.DefaultCulture = "en-US";
    opt.SupportedCultures = ["en-US", "tr-TR", "de-DE"];
    opt.FallbackToDefault = true;
})
.AddKckLocalizationJson(opt =>
{
    opt.ResourcePath = "Resources";
});
```

Dizin yapisi:

```
Resources/
  en-US.json
  tr-TR.json
  de-DE.json
```

`tr-TR.json` ornegi:

```json
{
  "Greeting": "Merhaba {0}",
  "ItemsFound": {
    "zero": "Hic urun yok",
    "one": "1 urun bulundu",
    "other": "{0} urun bulundu"
  }
}
```

## YAML Setup

```csharp
services.AddKckLocalization(opt => { /* ... */ })
    .AddKckLocalizationYaml(opt =>
    {
        opt.ResourcePath = "Resources";
    });
```

YAML formati JSON ile ayni anahtar hiyerarsisini destekler.

## Kullanim

```csharp
public class GreetingService(ILocalizationService l10n)
{
    public async Task<string> GreetAsync(string name, CancellationToken ct)
    {
        return await l10n.GetStringAsync("Greeting", "en-US", [name], ct);
    }

    public async Task<string> ItemCountAsync(int count, string culture, CancellationToken ct)
    {
        return await l10n.GetPluralStringAsync("ItemsFound", count, culture, ct);
    }
}
```

## Pluralization

`IPluralizer` CLDR plural kategorilerini kullanir:
- `zero`, `one`, `two`, `few`, `many`, `other`

Eksik kategori → `other` fallback'i.

## Formatter

`IFormatterService` `string.Format` sargısı + culture bilinirli formatlama.
Default implementasyon invariant culture yerine hedef culture'i kullanir.

## Fallback Zinciri

`FallbackToDefault = true` oldugunda:
1. Istenen culture'da ara (`tr-TR`)
2. Parent culture (`tr`)
3. Default culture (`en-US`)
4. Key'in kendisi (surec olmadi — gelistirici hatasi gorulebilir)

## Cache

Resource yuklemesi cache'lenir (process-local). Runtime'da dosya degistirirsen
resource provider'i yeniden yukleyen `IFeatureChangeNotifier` patterni ekle.
