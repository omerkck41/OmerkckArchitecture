namespace Kck.Core.Abstractions.Entities;

/// <summary>
/// Soft-delete contract. Entities are logically deleted rather than physically removed.
/// With Kck.Persistence.EntityFramework, call <c>modelBuilder.ApplyGlobalSoftDeleteQueryFilter()</c> to exclude them
/// from default queries (a named EF Core filter that <c>QueryOptions.IncludeDeleted</c> lifts).
/// </summary>
public interface ISoftDeletable
{
    /// <summary><see langword="true"/> when the entity has been soft-deleted.</summary>
    bool IsDeleted { get; set; }

    /// <summary>Username or identifier of who performed the soft-delete.</summary>
    string? DeletedBy { get; set; }

    /// <summary>UTC timestamp of the soft-delete operation.</summary>
    DateTime? DeletedDate { get; set; }
}
