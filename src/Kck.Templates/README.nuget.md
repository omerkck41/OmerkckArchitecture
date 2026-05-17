# Kck.Templates

`dotnet new` templates for Kck-based .NET projects — install once, scaffold a fully wired Kck WebAPI project in seconds.

## Installation

```bash
dotnet new install Kck.Templates
```

## Quick Start

```bash
# Scaffold a new WebAPI project pre-wired with Kck.Bundle.WebApi
dotnet new kck-webapi -n MyApp

cd MyApp

# Run immediately — JWT, caching, exception handling already configured
dotnet run
```

The generated project includes:

- `Program.cs` with `AddKckWebApiDefaults` + `UseKckWebApiDefaults`
- `appsettings.json` with placeholder Kck configuration sections
- A sample `WeatherForecast` controller demonstrating the Result pattern
- `Dockerfile` and `.github/workflows/build.yml` stubs

## Available Templates

| Template Name | Description |
|---|---|
| `kck-webapi` | ASP.NET Core WebAPI with `Kck.Bundle.WebApi` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/README.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
