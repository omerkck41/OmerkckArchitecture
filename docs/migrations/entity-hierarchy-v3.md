# Entity Hiyerarşisi — v2.x → v3.0 Migrasyon Kılavuzu

**ADR:** [ADR-0024](../adr/0024-entity-hierarchy.md)

---

## Özet

v3.0'da `Entity<TId>` üç ayrı sınıfa bölündü:

| v2.x | v3.0 | Ne zaman kullan |
|---|---|---|
| `Entity<TId>` | `Entity<TId>` | Yalnızca kimlik + domain events (nadiren kullanılır) |
| `Entity<TId>` | `AuditableEntity<TId>` | Kimlik + audit trail (oluşturma/değiştirme tarihleri) |
| `Entity<TId>` | `FullEntity<TId>` | Kimlik + audit trail + soft-delete + RowVersion |

**En yaygın migration:** `Entity<TId>` → `FullEntity<TId>` (v2.x'deki Entity ile birebir aynı)

---

## Adım Adım

### 1. `Entity<TId>`'yi `FullEntity<TId>` ile değiştir

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

### 2. Sadece audit isteyen entity'ler

```csharp
// Soft-delete gerekmiyorsa
public class LogEntry : AuditableEntity<Guid>
{
    public string Message { get; set; } = string.Empty;
}
```

### 3. Sadece kimlik isteyen entity'ler

```csharp
// Audit ve soft-delete gerekmiyorsa (nadiren)
public class Tag : Entity<int>
{
    public string Name { get; set; } = string.Empty;
}
```

### 4. EF Core konfigürasyonu

EF Core konfigürasyonunuzda `Entity<TId>` type constraint varsa güncelle:

```csharp
// v2.x
modelBuilder.Entity<Product>(b => b.HasKey(e => e.Id));

// v3.0 — değişiklik gerekmez; konfigürasyon aynı çalışır
// FullEntity<TId>, Entity<TId>'den türediğinden EF Core entity discovery otomatik çalışır
```

### 5. Repository generics (eğer kısıtlama varsa)

```csharp
// v2.x
public class MyRepo<T> where T : Entity<Guid> { }

// v3.0 — sadece audit/soft-delete isteyen entity'lerle çalışacaksa
public class MyRepo<T> where T : FullEntity<Guid> { }

// v3.0 — tüm entity'lerle çalışacaksa
public class MyRepo<T> where T : Entity<Guid> { }
```

---

## Kontrol Listesi

- [ ] Tüm `Entity<TId>` implementasyonları `FullEntity<TId>` (veya uygun base class) ile güncellendi
- [ ] EF Core configurations gözden geçirildi
- [ ] Repository generic constraints güncellendi (gerekiyorsa)
- [ ] `dotnet build` çıktısı 0 hata
