using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Kck.Core.Abstractions.Entities;

/// <summary>
/// Full-featured entity base class with audit trail, soft-delete, and optimistic concurrency.
/// This is the direct replacement for the v2.x <c>Entity&lt;TId&gt;</c> that combined all three concerns.
/// </summary>
/// <typeparam name="TId">Primary key type.</typeparam>
[DebuggerDisplay("Id={Id}, IsDeleted={IsDeleted}, CreatedBy={CreatedBy,nq}")]
public abstract class FullEntity<TId> : AuditableEntity<TId>, ISoftDeletable
{
    /// <summary>Initialises a new entity with a default (unset) identifier.</summary>
    protected FullEntity() { }

    /// <summary>Initialises a new entity with the specified identifier.</summary>
    /// <param name="id">The primary key value.</param>
    protected FullEntity(TId id) : base(id) { }

    /// <inheritdoc/>
    public virtual bool IsDeleted { get; set; }

    /// <inheritdoc/>
    public virtual string? DeletedBy { get; set; }

    /// <inheritdoc/>
    public virtual DateTime? DeletedDate { get; set; }

    /// <summary>EF Core concurrency token. Updated automatically on every save.</summary>
    [Timestamp]
    public virtual byte[]? RowVersion { get; set; }
}
