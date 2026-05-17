# Kck.Localization.Json

JSON resource file-backed localization provider implementing `IStringLocalizer` for multi-language ASP.NET Core applications.

## Installation

```bash
dotnet add package Kck.Localization.Json
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckLocalizationJson(options =>
{
    options.ResourcePath = "Resources";
    options.DefaultCulture = "en";
});

// Resources/en.json
// { "Greeting": "Hello, {0}!" }
// Resources/tr.json
// { "Greeting": "Merhaba, {0}!" }

// Usage in controller
public class HomeController(IStringLocalizer<HomeController> localizer)
{
    [HttpGet]
    public string Hello() => localizer["Greeting", "World"];
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ResourcePath` | Folder containing JSON locale files | `"Resources"` |
| `DefaultCulture` | Fallback culture when translation is missing | `"en"` |
| `CacheDuration` | How long parsed locale files are cached | `TimeSpan.FromHours(1)` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/localization.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
