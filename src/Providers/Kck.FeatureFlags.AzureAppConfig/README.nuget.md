# Kck.FeatureFlags.AzureAppConfig

Azure App Configuration-backed feature flag provider with automatic refresh for `IFeatureFlagService`.

## Installation

```bash
dotnet add package Kck.FeatureFlags.AzureAppConfig
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckFeatureFlagsAzureAppConfig(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("AZURE_APP_CONFIG")!;
    options.RefreshInterval = TimeSpan.FromMinutes(5);
});

// Usage
public class BillingController(IFeatureFlagService flags)
{
    [HttpPost("pay")]
    public async Task<IActionResult> Pay(CancellationToken ct)
    {
        if (!await flags.IsEnabledAsync("NewBillingFlow", ct))
            return BadRequest("Feature not available.");
        // ...
    }
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `ConnectionString` | Azure App Configuration connection string | required (or `Endpoint`) |
| `Endpoint` | Azure App Configuration endpoint URI (Managed Identity) | `null` |
| `RefreshInterval` | How often feature flags are refreshed | `TimeSpan.FromMinutes(5)` |
| `LabelFilter` | App Configuration label filter | `null` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/feature-flags.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
