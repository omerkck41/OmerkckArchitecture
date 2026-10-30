# Kck.Localization.Yaml

YAML resource file-backed localization provider implementing `IStringLocalizer` for human-friendly multi-language content management.

## Installation

```bash
dotnet add package Kck.Localization.Yaml
```

## Quick Start

```csharp
using Kck.Localization.Abstractions;

// Program.cs — the localization service plus this resource provider
builder.Services.AddKckLocalization(options =>
{
    options.ResourcePath = "Resources";
    options.DefaultCulture = "en";
});
builder.Services.AddKckLocalizationYaml();

// Resources/en.yaml
// greeting: "Hello, {0}!"
// Resources/tr.yaml
// greeting: "Merhaba, {0}!"

// Usage
public class WelcomeService(ILocalizationService localizer)
{
    public Task<string> GreetAsync(string name, string culture, CancellationToken ct) =>
        localizer.GetStringAsync("greeting", culture, [name], ct);
}
```

## Configuration

No options of its own: it reads `LocalizationOptions` (`ResourcePath`, caching) set with `AddKckLocalization(...)`. Files are `{ResourcePath}/{culture}.yaml` (or `.yml`).

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/localization.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
