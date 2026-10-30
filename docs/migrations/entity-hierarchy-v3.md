# Entity Hierarchy — v2.x → v3.0 Migration Guide

**ADR:** [ADR-0024](../adr/0024-entity-hierarchy.md)

---

## Summary

In v3.0, `Entity<TId>` was split into three separate classes:

| v2.x | v3.0 | When to use |
|---|---|---|
| `Entity<TId>` | `Entity<TId>` | Identity + domain events only (rarely used) |
| `Entity<TId>` | `AuditableEntity<TId>` | Identity + audit trail (creation/modification dates) |
| `Entity<TId>` | `FullEntity<TId>` | Identity + audit trail + soft-delete + RowVersion |

**Most common migration:** `Entity<TId>` → `FullEntity<TId>` (identical to the v2.x Entity)

---

## Step by Step

### 1. Replace `Entity<TId>` with `FullEntity<TId>`

v2.x:

```csharp
using Kck.Core.Abstractions.Entities;

public class Product : Entity<Guid>
{
    public Product(Guid id) : base(id) { }
    public string Name { get; set; } = string.Empty;
}
```

v3.0:

```csharp
using Kck.Core.Abstractions.Entities;

public class Product : FullEntity<Guid>
{
    public Product(Guid id) : base(id) { }
    public string Name { get; set; } = string.Empty;
}
```

`FullEntity<TId>.RowVersion` is a concurrency token only on SQL Server (`rowversion`); other providers do not generate it.

### 2. Entities that need audit only

```csharp
using Kck.Core.Abstractions.Entities;

// If you do not need soft-delete
public class LogEntry : AuditableEntity<Guid>
{
    public string Message { get; set; } = string.Empty;
}
```

### 3. Entities that need identity only

```csharp
using Kck.Core.Abstractions.Entities;

// If you need neither audit nor soft-delete (rare)
public class Tag : Entity<int>
{
    public string Name { get; set; } = string.Empty;
}
```

### 4. EF Core configuration

No change is needed: `modelBuilder.Entity<Product>(b => b.HasKey(e => e.Id))` works as before, and because
`FullEntity<TId>` derives from `Entity<TId>`, EF Core discovers the entities the same way.

### 5. Repository generics (if you have constraints)

A constraint on `Entity<TId>` still accepts every entity. Narrow it only where the code needs audit or soft-delete:

```csharp
using Kck.Core.Abstractions.Entities;

// Works with every entity (unchanged from v2.x)
public class MyRepo<T> where T : Entity<Guid> { }

// Only entities with audit trail and soft-delete
public class MyAuditedRepo<T> where T : FullEntity<Guid> { }
```

---

## Checklist

- [ ] All `Entity<TId>` implementations updated to `FullEntity<TId>` (or the appropriate base class)
- [ ] EF Core configurations reviewed
- [ ] Repository generic constraints updated (if needed)
- [ ] `dotnet build` output shows 0 errors
