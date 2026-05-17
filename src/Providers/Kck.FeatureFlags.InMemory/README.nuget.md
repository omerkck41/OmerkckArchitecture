# Kck.FeatureFlags.InMemory

In-memory `IFeatureFlagService` for local development, unit testing, and simple single-node feature toggling.

## Installation

```bash
dotnet add package Kck.FeatureFlags.InMemory
```

## Quick Start

```csharp
// Program.cs
builder.Services.AddKckFeatureFlagsInMemory(options =>
{
    options.Features["NewCheckout"] = true;
    options.Features["DarkMode"] = false;
});

// Usage
public class CheckoutService(IFeatureFlagService flags)
{
    public async Task<bool> UseNewFlowAsync(CancellationToken ct)
        => await flags.IsEnabledAsync("NewCheckout", ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Features` | Dictionary of feature name → enabled state | `{}` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/feature-flags.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
