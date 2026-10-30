# Kck.Localization

Core localization abstractions and culture-selection middleware for Kck applications — use with `Kck.Localization.Json` or `Kck.Localization.Yaml`.

## Installation

```bash
dotnet add package Kck.Localization
```

## Quick Start

```csharp
using Kck.Localization.Abstractions;

// Program.cs - the service, then a resource provider (Kck.Localization.Json or .Yaml)
builder.Services.AddKckLocalization(options =>
{
    options.ResourcePath = "Resources";
    options.DefaultCulture = "en";
    options.FallbackCulture = "en";
    options.SupportedCultures = ["en", "tr"];
});
builder.Services.AddKckLocalizationJson();

// Usage: requested culture -> parent culture -> FallbackCulture -> "[key]"
public class Greeter(ILocalizationService l10n)
{
    public Task<string> GreetAsync(string name, string culture, CancellationToken ct) =>
        l10n.GetStringAsync("greeting", culture, [name], ct);
}
```

## Configuration

`LocalizationOptions` (Kck.Localization.Abstractions):

| Property | Description | Default |
|---|---|---|
| `DefaultCulture` | Culture used when none is given | `"en"` |
| `FallbackCulture` | Last culture tried before the key is reported missing | `"en"` |
| `SupportedCultures` | Cultures the application supports | `["en"]` |
| `ResourcePath` | Folder of the JSON / YAML resource files | `"Resources"` |
| `EnableCaching` | Cache loaded resource files | `true` |
| `CacheExpiration` | How long a loaded file is cached (applied since 4.7.0) | 30 minutes |
| `ThrowOnMissing` | Throw `KeyNotFoundException` for a missing key | `false` |
| `MissingKeyPattern` | Returned for a missing key; `{0}` is the key | `"[{0}]"` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/localization.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
