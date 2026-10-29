using Kck.Core.Abstractions.Entities;
using Kck.Persistence.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Kck.Persistence.EntityFramework.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets audit fields on <see cref="IAuditable"/>
/// and converts hard deletes to soft deletes for <see cref="ISoftDeletable"/> entities, except deletes made
/// through a repository with <c>permanent: true</c>.
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

    /// <inheritdoc />
    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            PermanentDeletes.Clear(eventData.Context);

        return new ValueTask<int>(result);
    }

    /// <inheritdoc />
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is not null)
            PermanentDeletes.Clear(eventData.Context);

        return result;
    }

    // Soft deletes first, so the dependents they keep are not stamped as modified.
    private static void ApplyAuditFields(DbContext context)
    {
        var now = DateTime.UtcNow;
        var softDeleted = SoftDeleteHelper.SoftDeleteTrackedDeletes(context, now);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added when entry.Entity is IAuditable addedAuditable:
                    addedAuditable.CreatedDate = now;
                    break;

                case EntityState.Modified when entry.Entity is IAuditable modifiedAuditable
                                               && !softDeleted.Contains(entry.Entity):
                    modifiedAuditable.ModifiedDate = now;
                    break;
            }
        }
    }
}
