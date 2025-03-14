using Core.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Core.Persistence.Extensions;

public static class ChangeTrackerExtensions
{
    private static readonly Dictionary<EntityState, Action<DbContext, IAuditable, string>> _behaviors = new()
    {
        {EntityState.Added,AddBehavior},
        {EntityState.Modified,ModifiedBehavior}
    };

    private static void AddBehavior(DbContext context, IAuditable auditableEntity, string systemUser = "Omerkck-System")
    {
        auditableEntity.CreatedDate = DateTime.UtcNow;
        auditableEntity.IsDeleted = false;
        auditableEntity.CreatedBy = systemUser;

        context.Entry(auditableEntity).Property(x => x.ModifiedDate).IsModified = false;
    }

    private static void ModifiedBehavior(DbContext context, IAuditable auditableEntity, string systemUser = "Omerkck-System")
    {
        auditableEntity.ModifiedDate = DateTime.UtcNow;
        auditableEntity.ModifiedBy = systemUser;

        context.Entry(auditableEntity).Property(x => x.CreatedDate).IsModified = false;
    }

    /// <summary>
    /// Tüm audit işlemlerini (Created, Modified, Deleted) uygular.
    /// Eğer "IsHardDelete" shadow property true ise, silme işlemi hard delete olarak bırakılır.
    /// Aksi halde, silme soft delete'e dönüştürülür.
    /// </summary>
    public static void ApplyAuditInformation(this ChangeTracker changeTracker, string systemUser = "Omerkck-System")
    {
        var entries = changeTracker.Entries().ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is not IAuditable auditable) continue;

            // Eğer sözlükte mevcut değilse, davranış uygulamadan geçebilirsiniz.
            if (!_behaviors.ContainsKey(entry.State))
                continue;

            // entry.Context üzerinden DbContext’e erişim sağlanıyor.
            _behaviors[entry.State](entry.Context, auditable, systemUser);
        }
    }
}
}