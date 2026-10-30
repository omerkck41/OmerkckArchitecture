# Kck.Localization.Json

JSON resource file-backed localization provider implementing `IStringLocalizer` for multi-language ASP.NET Core applications.

## Installation

```bash
dotnet add package Kck.Localization.Json
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
builder.Services.AddKckLocalizationJson();

// Resources/en.json
// { "greeting": "Hello, {0}!" }
// Resources/tr.json
// { "greeting": "Merhaba, {0}!" }

// Usage
public class WelcomeService(ILocalizationService localizer)
{
    public Task<string> GreetAsync(string name, string culture, CancellationToken ct) =>
        localizer.GetStringAsync("greeting", culture, [name], ct);
}
```

## Configuration

No options of its own: it reads `LocalizationOptions` (`ResourcePath`, caching) set with `AddKckLocalization(...)`. Files are `{ResourcePath}/{culture}.json`; nested objects become dotted keys.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/localization.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
