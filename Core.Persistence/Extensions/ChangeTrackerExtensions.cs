using Core.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Core.Persistence.Extensions;

public static class ChangeTrackerExtensions
{
    /// <summary>
    /// Tüm audit işlemlerini (Created, Modified, Deleted) uygular.
    /// Eğer "IsHardDelete" shadow property true ise, silme işlemi hard delete olarak bırakılır.
    /// Aksi halde, silme soft delete'e dönüştürülür.
    /// </summary>
    public static void ApplyAuditInformation(this ChangeTracker changeTracker, string systemUser = "ArchonApp-System")
    {
        var entries = changeTracker.Entries<Entity<int>>()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    entry.Entity.IsDeleted = false;
                    entry.Entity.CreatedBy = systemUser;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedDate = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = systemUser;
                    break;

                case EntityState.Deleted:
                    // Eğer isHardDelete true ise, entry state'i Deleted olarak bırakılır.
                    break;
            }
        }
    }
}