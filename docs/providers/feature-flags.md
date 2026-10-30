# Feature Flags

`IFeatureFlagService` is an abstraction for runtime feature toggling. The in-memory provider reads flags from options
(or configuration); the Azure App Configuration provider reads Azure's feature flags. For another service, writing an
adapter is straightforward (see [Extending](#extending)).

## Packages

| Package | Target |
|---|---|
| `Kck.FeatureFlags.Abstractions` | `IFeatureFlagService`, `FeatureDefinition`, `IFeatureContext` |
| `Kck.FeatureFlags.InMemory` | Dictionary from options or configuration |
| `Kck.FeatureFlags.AzureAppConfig` | Azure App Configuration feature flags |

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
builder.Services.AddKckFeatureFlagsInMemory(opt =>
    builder.Configuration.GetSection("Features").Bind(opt.Features));
```

## Azure App Configuration

```csharp
builder.Services.AddKckFeatureFlagsAzureAppConfig(opt =>
{
    opt.Endpoint = builder.Configuration["AppConfig:Endpoint"]; // DefaultAzureCredential (Managed Identity, Azure CLI)
    // or: opt.ConnectionString = builder.Configuration["AppConfig:ConnectionString"];
    opt.Label = "production";                                  // optional
    opt.RefreshInterval = TimeSpan.FromSeconds(30);            // default
});
```

Flags are read from keys under `.appconfig.featureflag/` (`KeyPrefix`) and cached for `RefreshInterval`.
`GetValueAsync<T>` deserializes a flag's `parameters` object, or returns the default value.

## Usage

```csharp
using Kck.FeatureFlags.Abstractions;

app.MapGet("/checkout", async (IFeatureFlagService flags, CancellationToken ct) =>
    await flags.IsEnabledAsync("new-checkout-ui", ct)
        ? Results.Ok("checkout v2")
        : Results.Ok("checkout v1"));
```

An unknown flag is disabled.

## Context-Aware Check

`IsEnabledAsync(name, context)` accepts the user and tenant of the request:

```csharp
using Kck.FeatureFlags.Abstractions;

public sealed record RequestFeatureContext(string? UserId, string? TenantId) : IFeatureContext
{
    public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
}

public sealed class BetaGate(IFeatureFlagService flags)
{
    public Task<bool> IsOpenAsync(string userId, CancellationToken ct) =>
        flags.IsEnabledAsync("beta-feature", new RequestFeatureContext(userId, TenantId: null), ct);
}
```

> Neither built-in provider evaluates the context yet: both return the flag's plain on/off value (Azure targeting and
> time-window filters are read as `FilterType` but not applied). For per-user rollout, implement
> `IFeatureFlagService` over a service that supports it.

## List All Flags

```csharp
using Kck.FeatureFlags.Abstractions;

public sealed class FlagReport(IFeatureFlagService flags, ILogger<FlagReport> logger)
{
    public async Task LogAllAsync(CancellationToken ct)
    {
        foreach (var flag in await flags.GetAllAsync(ct))
            logger.LogInformation("{Flag}: {Enabled}", flag.Name, flag.Enabled);
    }
}
```

## Extending

For another provider (LaunchDarkly, ConfigCat):

1. Implement `IFeatureFlagService`.
2. Write an `AddKckFeatureFlags<YourProvider>()` extension method.
3. Register it with `TryAddSingleton<IFeatureFlagService, YourProviderService>()`.
