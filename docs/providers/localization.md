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
    opt.SupportedCultures = ["en-US", "tr-TR", "de-DE"];
    opt.FallbackToDefault = true;
})
.AddKckLocalizationJson(opt =>
{
    opt.ResourcePath = "Resources";
});
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
services.AddKckLocalization(opt => { /* ... */ })
    .AddKckLocalizationYaml(opt =>
    {
        opt.ResourcePath = "Resources";
    });
```

The YAML format supports the same key hierarchy as JSON.

## Usage

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

`IPluralizer` uses the CLDR plural categories:
- `zero`, `one`, `two`, `few`, `many`, `other`

A missing category falls back to `other`.

## Formatter

`IFormatterService` is a `string.Format` wrapper with culture-aware formatting.
The default implementation uses the target culture instead of the invariant culture.

## Fallback Chain

When `FallbackToDefault = true`:
1. Look up the requested culture (`tr-TR`).
2. Parent culture (`tr`).
3. Default culture (`en-US`).
4. The key itself (no exception is thrown, so a developer error stays visible).

## Cache

Resource loading is cached (process-local). If you change files at runtime,
add an `IFeatureChangeNotifier` pattern that reloads the resource provider.
