# Kck.FeatureFlags.Abstractions

Provider-agnostic feature flag abstractions for runtime feature toggling — evaluate flags with optional per-user or per-tenant context.

## Installation

```bash
dotnet add package Kck.FeatureFlags.Abstractions
```

## Quick Start

```csharp
// Program.cs — register a concrete provider (e.g. Kck.FeatureFlags.InMemory)
builder.Services.AddKckInMemoryFeatureFlags(flags =>
{
    flags.Add("NewCheckout", enabled: true);
    flags.Add("BetaDashboard", enabled: false);
});

// Use IFeatureFlagService in a controller or handler
public class CheckoutController(IFeatureFlagService flags)
{
    [HttpPost("checkout")]
    public async Task<IActionResult> CheckoutAsync(
        CheckoutRequest request, CancellationToken ct)
    {
        var context = new FeatureContext { UserId = request.UserId };

        if (await flags.IsEnabledAsync("NewCheckout", context, ct))
            return await RunNewCheckoutAsync(request, ct);

        return await RunLegacyCheckoutAsync(request, ct);
    }
}

// List all defined flags
var allFlags = await flags.GetAllAsync(ct);
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
