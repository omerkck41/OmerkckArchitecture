# Kck.FeatureFlags.Abstractions

Provider-agnostic feature flag abstractions for runtime feature toggling — evaluate flags with optional per-user or per-tenant context.

## Installation

```bash
dotnet add package Kck.FeatureFlags.Abstractions
```

## Quick Start

```csharp
using Kck.FeatureFlags.Abstractions;

// Program.cs — a provider (e.g. Kck.FeatureFlags.InMemory)
builder.Services.AddKckFeatureFlagsInMemory(flags =>
{
    flags.Features["NewCheckout"] = true;
    flags.Features["BetaDashboard"] = false;
});

// Use IFeatureFlagService anywhere
public class CheckoutService(IFeatureFlagService flags)
{
    public async Task<string> CheckoutAsync(CancellationToken ct) =>
        await flags.IsEnabledAsync("NewCheckout", ct) ? "new checkout" : "legacy checkout";

    public async Task<IReadOnlyList<string>> EnabledFlagsAsync(CancellationToken ct) =>
        (await flags.GetAllAsync(ct)).Where(f => f.Enabled).Select(f => f.Name).ToList();
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `FeatureFlags:Provider` | `InMemory` or `AzureAppConfiguration` | `InMemory` |
| `FeatureFlags:RefreshIntervalSeconds` | How often remote flags are refreshed | `60` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/feature-flags.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
