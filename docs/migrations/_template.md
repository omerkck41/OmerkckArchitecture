# Migration vX.x → vY.0

**Release date:** YYYY-MM-DD
**Impact:** Breaking
**Estimated effort:** S / M / L (depends on the size of your codebase)

## Why This Major?

Explain the reason for this major in 1-2 paragraphs. Which architecture
decision (ADR-XXXX) triggered it? Give the rationale.

## Affected Packages

| Package | Change type | Impact level |
|---|---|---|
| `Kck.Core.Abstractions` | API rename | Medium |
| `Kck.Caching.Redis` | Service registration signature | High |

(Impact: Low = interface implementations only, Medium = call sites,
High = configuration / DI level)

## At a Glance (TL;DR)

- Change 1 (in 3-5 words)
- Change 2
- Change 3

## Detailed Changes

### 1. `<API name>` Removed/Renamed

**Reason:** ...

**Before (vX):**
```csharp
services.AddKckXxx(opt => opt.Foo = "bar");
```

**After (vY):**
```csharp
services.AddKckXxx(opt => opt.Bar = "foo");
```

**DiagnosticId:** `KCKnnnn` (if any)

### 2. `<API name>` Behavior Change

...

## Automated Migration

Conversions that can be automated:

- **Roslynator analyzer:** `<analyzer-id>` — can be applied with
  `dotnet format analyzers`.
- **`dotnet format`:** ...
- **IDE quick fix:** Visual Studio / Rider Ctrl+. / Alt+Enter — click the
  Obsolete warning, then "Use suggested replacement".

## Manual Steps

Places that cannot be automated:

1. Rename configuration (`appsettings.json`) keys: `Foo` → `Bar`.
2. ...

## References

- ADR-XXXX
- CHANGELOG.md `[vY.0.0]`
- `docs/policies/versioning.md`
