# Kck.FeatureFlags.AzureAppConfig

Azure App Configuration-backed feature flag provider with automatic refresh for `IFeatureFlagService`.

## Installation

```bash
dotnet add package Kck.FeatureFlags.AzureAppConfig
```

## Quick Start

```csharp
using Kck.FeatureFlags.Abstractions;

// Program.cs
builder.Services.AddKckFeatureFlagsAzureAppConfig(options =>
{
    options.Endpoint = builder.Configuration["AppConfig:Endpoint"]; // Managed Identity (DefaultAzureCredential)
    // or: options.ConnectionString = Environment.GetEnvironmentVariable("AZURE_APP_CONFIG");
    options.RefreshInterval = TimeSpan.FromMinutes(5);
});

// Usage
app.MapPost("/pay", async (IFeatureFlagService flags, CancellationToken ct) =>
    await flags.IsEnabledAsync("NewBillingFlow", ct)
        ? Results.Ok("new billing flow")
        : Results.BadRequest("Feature not available."));
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ConnectionString` | App Configuration connection string | `null` (set this or `Endpoint`) |
| `Endpoint` | App Configuration endpoint, with `DefaultAzureCredential` | `null` |
| `KeyPrefix` | Prefix of the feature flag keys | `".appconfig.featureflag/"` |
| `Label` | Label filter | `null` |
| `RefreshInterval` | How long the flags are cached before they are read again | 30 s |

The request context passed to `IsEnabledAsync(name, context)` is not evaluated: targeting and time-window filters are read but not applied.

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/feature-flags.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
