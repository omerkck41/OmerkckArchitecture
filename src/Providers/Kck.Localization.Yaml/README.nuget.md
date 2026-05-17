# Kck.Localization.Yaml

YAML resource file-backed localization provider implementing `IStringLocalizer` for human-friendly multi-language content management.

## Installation

```bash
dotnet add package Kck.Localization.Yaml
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckLocalizationYaml(options =>
{
    options.ResourcePath = "Resources";
    options.DefaultCulture = "en";
});

// Resources/en.yaml
// greeting: "Hello, {0}!"
// Resources/tr.yaml
// greeting: "Merhaba, {0}!"

// Usage
public class WelcomeService(IStringLocalizer<WelcomeService> localizer)
{
    public string Greet(string name) => localizer["greeting", name];
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ResourcePath` | Folder containing YAML locale files | `"Resources"` |
| `DefaultCulture` | Fallback culture when translation is missing | `"en"` |
| `FileExtension` | YAML file extension pattern | `".yaml"` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/localization.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
