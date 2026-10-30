# Localization

Multilingual resource management. `ILocalizationService` provides cultured
string lookup, pluralization and parameter interpolation.

## Packages

| Package | Target |
|---|---|
| `Kck.Localization.Abstractions` | `ILocalizationService`, `IResourceProvider`, `IPluralizer`, `IFormatterService` |
| `Kck.Localization` | Core service + default pluralizer/formatter |
| `Kck.Localization.Json` | JSON file resource provider |
| `Kck.Localization.Yaml` | YAML file resource provider |

## JSON Setup

```csharp
services.AddKckLocalization(opt =>
{
    opt.DefaultCulture = "en-US";
    opt.FallbackCulture = "en-US";
    opt.SupportedCultures = ["en-US", "tr-TR", "de-DE"];
    opt.ResourcePath = "Resources";   // default; read relative to the working directory
});
services.AddKckLocalizationJson();
```

Directory structure:

```
Resources/
  en-US.json
  tr-TR.json
  de-DE.json
```

`tr-TR.json` example:

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
services.AddKckLocalization(opt => opt.ResourcePath = "Resources");
services.AddKckLocalizationYaml();
```

The YAML format supports the same key hierarchy as JSON.

## Usage

```csharp
using Kck.Localization.Abstractions;

public class GreetingService(ILocalizationService l10n)
{
    public Task<string> GreetAsync(string name, string culture, CancellationToken ct) =>
        l10n.GetStringAsync("Greeting", culture, [name], ct);

    public Task<string> ItemCountAsync(int count, string culture, CancellationToken ct) =>
        l10n.GetPluralStringAsync("ItemsFound", count, culture, ct); // reads ItemsFound.one, ItemsFound.other...
}
```

## Pluralization

`IPluralizer` uses the CLDR plural categories:
- `zero`, `one`, `two`, `few`, `many`, `other`

A missing category falls back to `other`.

## Formatter

`IFormatterService` is a `string.Format` wrapper with culture-aware formatting.
The default implementation uses the target culture instead of the invariant culture.

## Fallback Chain

1. The requested culture (`tr-TR`).
2. Its parent culture (`tr`).
3. `FallbackCulture` (`en` by default).
4. Not found: the key formatted with `MissingKeyPattern` (`[Greeting]` by default), so a missing translation stays
   visible; with `ThrowOnMissing = true`, a `KeyNotFoundException` instead.

## Cache

Resource files are loaded once and cached per process (`EnableCaching`; `CacheExpiration` is not applied yet). After changing files at
runtime, call `ILocalizationService.ReloadAsync()`: it reloads the JSON and YAML providers.
