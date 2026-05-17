namespace Kck.Core.Abstractions.Entities;

/// <summary>
/// Soft-delete contract. Entities are logically deleted rather than physically removed.
/// Add <c>HasQueryFilter(e =&gt; !e.IsDeleted)</c> in EF Core to exclude them from default queries.
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
