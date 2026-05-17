# Kck.Localization.Abstractions

Culture-aware localization abstractions with dynamic reload — look up translated strings by key and culture without coupling to JSON or YAML resource formats.

## Installation

```bash
dotnet add package Kck.Localization.Abstractions
```

## Quick Start

```csharp
// Program.cs — register a concrete provider (e.g. Kck.Localization.Json)
builder.Services.AddKckJsonLocalization(options =>
{
    options.ResourcePath = "Resources";
    options.DefaultCulture = "en";
});

// Use ILocalizationService in a handler or middleware
public class NotificationHandler(ILocalizationService localizer)
{
    public async Task<string> GetWelcomeMessageAsync(
        string userId, string culture, CancellationToken ct)
    {
        // Simple key lookup
        return await localizer.GetStringAsync("welcome.message", culture, ct);
    }

    public async Task<string> GetFormattedMessageAsync(
        string key, string culture, object[] args, CancellationToken ct)
    {
        // Parameterized lookup — {0}, {1} substituted from args
        return await localizer.GetStringAsync(key, culture, args, ct);
    }
}

// Force reload of resource files (e.g. after hot-update)
await localizer.ReloadAsync(ct);
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
