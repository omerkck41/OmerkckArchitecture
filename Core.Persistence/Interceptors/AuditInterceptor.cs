using Core.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Core.Persistence.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly string _systemUser;

    private static readonly Dictionary<EntityState, Action<DbContext, IAuditable, string>> _behaviors = new()
    {
        { EntityState.Added, AddBehavior },
        { EntityState.Modified, ModifiedBehavior },
        { EntityState.Deleted, DeletedBehavior }
    };

    private static void AddBehavior(DbContext context, IAuditable auditableEntity, string systemUser = "Omerkck-System")
    {
        auditableEntity.CreatedDate = DateTime.UtcNow;
        auditableEntity.IsDeleted = false;
        auditableEntity.CreatedBy = systemUser;

        // ModifiedDate için değişiklik yapılmamasını sağlıyoruz.
        context.Entry(auditableEntity).Property(x => x.ModifiedDate).IsModified = false;
    }

    private static void ModifiedBehavior(DbContext context, IAuditable auditableEntity, string systemUser = "Omerkck-System")
    {
        auditableEntity.ModifiedDate = DateTime.UtcNow;
        auditableEntity.ModifiedBy = systemUser;

        // Oluşturma bilgileri değişmemeli.
        context.Entry(auditableEntity).Property(x => x.CreatedDate).IsModified = false;
    }

    private static void DeletedBehavior(DbContext context, IAuditable auditableEntity, string systemUser = "Omerkck-System")
    {
        // Örneğin, soft delete uygulaması:
        //auditableEntity.DeletedDate = DateTime.UtcNow;
        //auditableEntity.DeletedBy = systemUser;
        //auditableEntity.IsDeleted = true;

        // EntityState’i Modified'a çevirerek veritabanına soft delete bilgisini yansıtıyoruz.
        //context.Entry(auditableEntity).State = EntityState.Modified;
    }

    public AuditInterceptor(string systemUser = "Omerkck-System") => _systemUser = systemUser;

    private void UpdateAuditProperties(DbContext? context)
    {
        if (context == null)
            return;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable &&
                       (e.State == EntityState.Added ||
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted));

        foreach (var entry in entries)
        {
            if (entry.Entity is IAuditable auditable)
            {
                if (_behaviors.TryGetValue(entry.State, out var behavior))
                {
                    behavior(context, auditable, _systemUser);
                }
            }
        }
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditProperties(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditProperties(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}