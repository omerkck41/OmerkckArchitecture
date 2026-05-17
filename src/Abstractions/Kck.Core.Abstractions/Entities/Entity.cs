using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Kck.Core.Abstractions.Entities;

/// <summary>
/// Base class for all domain entities. Combines identity (<see cref="IEntity{TId}"/>),
/// audit trail (<see cref="IAuditable"/>), soft-delete (<see cref="ISoftDeletable"/>),
/// optimistic concurrency (<see cref="RowVersion"/>), and domain event collection.
/// </summary>
/// <typeparam name="TId">Primary key type (e.g. <see cref="Guid"/>, <see langword="int"/>).</typeparam>
[DebuggerDisplay("Id={Id}, IsDeleted={IsDeleted}, CreatedBy={CreatedBy,nq}")]
public abstract class Entity<TId> : IEntity<TId>, IAuditable, ISoftDeletable
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private TId _id;

    /// <summary>Initialises a new entity with a default (unset) identifier.</summary>
    protected Entity() { _id = default!; }

    /// <summary>Initialises a new entity with the specified identifier.</summary>
    /// <param name="id">The primary key value.</param>
    protected Entity(TId id) { _id = id; }

    /// <inheritdoc/>
    public virtual TId Id { get => _id; set => _id = value; }

    /// <inheritdoc/>
    public virtual string CreatedBy { get; set; } = string.Empty;
    /// <inheritdoc/>
    public virtual DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    /// <inheritdoc/>
    public virtual string? ModifiedBy { get; set; }
    /// <inheritdoc/>
    public virtual DateTime? ModifiedDate { get; set; }

    /// <inheritdoc/>
    public virtual bool IsDeleted { get; set; }
    /// <inheritdoc/>
    public virtual string? DeletedBy { get; set; }
    /// <inheritdoc/>
    public virtual DateTime? DeletedDate { get; set; }

    /// <summary>EF Core concurrency token. Updated automatically on every save.</summary>
    [Timestamp]
    public virtual byte[]? RowVersion { get; set; }

    /// <summary>Read-only collection of uncommitted domain events raised by this entity.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Adds a domain event to be dispatched after the entity is persisted.</summary>
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Removes a previously added domain event.</summary>
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    /// <summary>Clears all pending domain events (called by the dispatcher after delivery).</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
