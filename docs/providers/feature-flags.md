# Feature Flags

`IFeatureFlagService` is an abstraction for runtime feature toggling.
The in-memory provider supports configuration-based flags; for larger projects,
writing a LaunchDarkly / Flagsmith adapter is straightforward.

## Packages

| Package | Target |
|---|---|
| `Kck.FeatureFlags.Abstractions` | `IFeatureFlagService`, `FeatureDefinition`, `IFeatureContext` |
| `Kck.FeatureFlags.InMemory` | Configuration-based dictionary |

## InMemory Setup

```csharp
services.AddKckFeatureFlagsInMemory(opt =>
{
    opt.Features["new-checkout-ui"] = true;
    opt.Features["legacy-export"] = false;
});
```

appsettings.json example:

```json
{
  "Features": {
    "new-checkout-ui": true,
    "legacy-export": false
  }
}
```

```csharp
services.AddKckFeatureFlagsInMemory(opt =>
{
    builder.Configuration.GetSection("Features").Bind(opt.Features);
});
```

## Usage

```csharp
public class CheckoutController(IFeatureFlagService flags)
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (await flags.IsEnabledAsync("new-checkout-ui", ct))
            return View("CheckoutV2");
        return View("CheckoutV1");
    }
}
```

## Context-Aware Check

For some scenarios (user segment, tenant):

```csharp
public sealed class RequestFeatureContext(string userId) : IFeatureContext
{
    public string? UserId { get; } = userId;
    public string? TenantId { get; } = null;
    public IReadOnlyDictionary<string, string> Properties { get; } =
        new Dictionary<string, string>();
}

var enabled = await flags.IsEnabledAsync("beta-feature", new RequestFeatureContext(userId));
```

> Note: `InMemoryFeatureFlagService` does not currently evaluate the context.
> A flag is only a boolean. For context-aware scenarios, implement a custom
> `IFeatureEvaluator`.

## List All Flags

```csharp
var all = await flags.GetAllAsync();
foreach (var flag in all)
    Console.WriteLine($"{flag.Name}: {flag.Enabled}");
```

## Extending

For another provider (LaunchDarkly, ConfigCat):

1. Implement `IFeatureFlagService`.
2. Write an `AddKckFeatureFlags<YourProvider>()` extension.
3. Register `TryAddSingleton<IFeatureFlagService, YourProviderService>()`.
