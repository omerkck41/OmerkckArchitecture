using System.Diagnostics;

namespace Kck.Core.Abstractions.Entities;

/// <summary>
/// Entity base class with audit trail. Extends <see cref="Entity{TId}"/> with
/// <see cref="IAuditable"/> fields populated automatically by <c>AuditInterceptor</c>.
/// Extend <see cref="FullEntity{TId}"/> if soft-delete is also needed.
/// </summary>
/// <typeparam name="TId">Primary key type.</typeparam>
[DebuggerDisplay("Id={Id}, CreatedBy={CreatedBy,nq}")]
public abstract class AuditableEntity<TId> : Entity<TId>, IAuditable
{
    /// <summary>Initialises a new entity with a default (unset) identifier.</summary>
    protected AuditableEntity() { }

    /// <summary>Initialises a new entity with the specified identifier.</summary>
    /// <param name="id">The primary key value.</param>
    protected AuditableEntity(TId id) : base(id) { }

    /// <inheritdoc/>
    public virtual string CreatedBy { get; set; } = string.Empty;

    /// <inheritdoc/>
    public virtual DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public virtual string? ModifiedBy { get; set; }

    /// <inheritdoc/>
    public virtual DateTime? ModifiedDate { get; set; }
}
