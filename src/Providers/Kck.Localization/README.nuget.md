# Kck.Localization

Core localization abstractions and culture-selection middleware for Kck applications — use with `Kck.Localization.Json` or `Kck.Localization.Yaml`.

## Installation

```bash
dotnet add package Kck.Localization
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckLocalization();

var app = builder.Build();
app.UseKckRequestLocalization();

// Combine with a resource provider
builder.Services.AddKckLocalizationJson(options =>
{
    options.ResourcePath = "Resources";
    options.DefaultCulture = "en";
});
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `SupportedCultures` | List of accepted culture codes | `["en", "tr"]` |
| `DefaultCulture` | Fallback culture | `"en"` |
| `CultureSelectionMode` | `Header`, `QueryString`, or `Cookie` | `Header` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/localization.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
