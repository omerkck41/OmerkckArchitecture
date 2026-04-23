using Kck.Core.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Kck.Persistence.EntityFramework.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit fields on <see cref="IAuditable"/>
/// and converts hard deletes to soft deletes for <see cref="ISoftDeletable"/> entities.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            ApplyAuditFields(eventData.Context);

        return new ValueTask<InterceptionResult<int>>(result);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            ApplyAuditFields(eventData.Context);

        return result;
    }

    private static void ApplyAuditFields(DbContext context)
    {
        var entries = context.ChangeTracker.Entries().ToList();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added when entry.Entity is IAuditable addedAuditable:
                    addedAuditable.CreatedDate = DateTime.UtcNow;
                    break;

                case EntityState.Modified when entry.Entity is IAuditable modifiedAuditable:
                    modifiedAuditable.ModifiedDate = DateTime.UtcNow;
                    break;

                case EntityState.Deleted when entry.Entity is ISoftDeletable softDeletable:
                    softDeletable.IsDeleted = true;
                    softDeletable.DeletedDate = DateTime.UtcNow;
                    entry.State = EntityState.Modified;
                    break;
            }
        }
    }
}
