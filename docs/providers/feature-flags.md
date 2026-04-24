# Feature Flags

`IFeatureFlagService` runtime ozellik togglelamasi icin abstraction.
In-memory provider konfigurasyon-tabanli flag'ler destekler; buyuk projelerde
LaunchDarkly / Flagsmith adapter yazmak kolay.

## Paketler

| Paket | Hedef |
|---|---|
| `Kck.FeatureFlags.Abstractions` | `IFeatureFlagService`, `FeatureDefinition`, `IFeatureContext` |
| `Kck.FeatureFlags.InMemory` | Konfigurasyon-tabanli dictionary |

## InMemory Setup

```csharp
services.AddKckFeatureFlagsInMemory(opt =>
{
    opt.Features["new-checkout-ui"] = true;
    opt.Features["legacy-export"] = false;
});
```

appsettings.json ornegi:

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

## Kullanim

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

Bazi sonlanana rekloler icin (user segment, tenant):

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

> Not: `InMemoryFeatureFlagService` context'i su an degerlendirmiyor — flag
> yalnizca boolean. Context-aware senaryolar icin ozel `IFeatureEvaluator`
> implement edin.

## Tum Flag'leri Listele

```csharp
var all = await flags.GetAllAsync();
foreach (var flag in all)
    Console.WriteLine($"{flag.Name}: {flag.Enabled}");
```

## Genisletme

Baska bir provider (LaunchDarkly, ConfigCat) icin:

1. `IFeatureFlagService` implement et
2. `AddKckFeatureFlags<YourProvider>()` extension yaz
3. `TryAddSingleton<IFeatureFlagService, YourProviderService>()` kaydet
