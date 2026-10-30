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

```csharp
// v2.x
public class Product : Entity<Guid>
{
    public Product(Guid id) : base(id) { }
    public string Name { get; set; } = string.Empty;
}

// v3.0
public class Product : FullEntity<Guid>
{
    public Product(Guid id) : base(id) { }
    public string Name { get; set; } = string.Empty;
}
```

### 2. Entities that need audit only

```csharp
// If you do not need soft-delete
public class LogEntry : AuditableEntity<Guid>
{
    public string Message { get; set; } = string.Empty;
}
```

### 3. Entities that need identity only

```csharp
// If you need neither audit nor soft-delete (rare)
public class Tag : Entity<int>
{
    public string Name { get; set; } = string.Empty;
}
```

### 4. EF Core configuration

If your EF Core configuration has an `Entity<TId>` type constraint, update it:

```csharp
// v2.x
modelBuilder.Entity<Product>(b => b.HasKey(e => e.Id));

// v3.0 — no change needed; the configuration works as before
// FullEntity<TId> derives from Entity<TId>, so EF Core entity discovery works automatically
```

### 5. Repository generics (if you have constraints)

```csharp
// v2.x
public class MyRepo<T> where T : Entity<Guid> { }

// v3.0 — if it will only work with entities that need audit/soft-delete
public class MyRepo<T> where T : FullEntity<Guid> { }

// v3.0 — if it will work with all entities
public class MyRepo<T> where T : Entity<Guid> { }
```

---

## Checklist

- [ ] All `Entity<TId>` implementations updated to `FullEntity<TId>` (or the appropriate base class)
- [ ] EF Core configurations reviewed
- [ ] Repository generic constraints updated (if needed)
- [ ] `dotnet build` output shows 0 errors
