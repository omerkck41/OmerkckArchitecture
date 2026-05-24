using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Kck.Core.Abstractions.Entities;

/// <summary>
/// Base class for all domain entities. Carries only the primary key and domain events.
/// Extend <see cref="AuditableEntity{TId}"/> to add audit trail,
/// or <see cref="FullEntity{TId}"/> to add audit trail and soft-delete.
/// </summary>
/// <typeparam name="TId">Primary key type (e.g. <see cref="Guid"/>, <see langword="int"/>).</typeparam>
[DebuggerDisplay("Id={Id}")]
public abstract class Entity<TId> : IEntity<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private TId _id;

    /// <summary>Initialises a new entity with a default (unset) identifier.</summary>
    protected Entity() { _id = default!; }

    /// <summary>Initialises a new entity with the specified identifier.</summary>
    /// <param name="id">The primary key value.</param>
    protected Entity(TId id) { _id = id; }

    /// <inheritdoc/>
    public TId Id { get => _id; set => _id = value; }

    /// <summary>Read-only collection of uncommitted domain events raised by this entity.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Adds a domain event to be dispatched after the entity is persisted.</summary>
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Removes a previously added domain event.</summary>
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    /// <summary>Clears all pending domain events (called by the dispatcher after delivery).</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
