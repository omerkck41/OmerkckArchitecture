# Kck.Localization.Abstractions

Culture-aware localization abstractions with dynamic reload — look up translated strings by key and culture without coupling to JSON or YAML resource formats.

## Installation

```bash
dotnet add package Kck.Localization.Abstractions
```

## Quick Start

```csharp
using Kck.Localization.Abstractions;

// Program.cs — the service and a resource provider (Kck.Localization.Json or .Yaml)
builder.Services.AddKckLocalization(options =>
{
    options.ResourcePath = "Resources";   // Resources/en.json, Resources/tr.json ...
    options.DefaultCulture = "en";
});
builder.Services.AddKckLocalizationJson();

// Use ILocalizationService in a handler
public class NotificationHandler(ILocalizationService localizer)
{
    public Task<string> WelcomeAsync(string culture, CancellationToken ct) =>
        localizer.GetStringAsync("welcome.message", culture, ct);

    public Task<string> GreetAsync(string name, string culture, CancellationToken ct) =>
        localizer.GetStringAsync("greeting", culture, [name], ct);   // "Hello {0}"

    public Task ReloadAsync(CancellationToken ct) => localizer.ReloadAsync(ct); // pick up changed files
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Localization:DefaultCulture` | Fallback culture when key missing | `"en"` |
| `Localization:ResourcePath` | Directory containing resource files | `"Resources"` |
| `Localization:EnableReload` | Watch files and reload on change | `true` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/localization.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
