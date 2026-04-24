using System.ComponentModel.DataAnnotations;

namespace Kck.Core.Abstractions.Entities;

public abstract class Entity<TId> : IEntity<TId>, IAuditable, ISoftDeletable
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private TId _id;

    protected Entity() { _id = default!; }
    protected Entity(TId id) { _id = id; }

    public virtual TId Id { get => _id; set => _id = value; }

    // IAuditable
    public virtual string CreatedBy { get; set; } = string.Empty;
    public virtual DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public virtual string? ModifiedBy { get; set; }
    public virtual DateTime? ModifiedDate { get; set; }

    // ISoftDeletable
    public virtual bool IsDeleted { get; set; }
    public virtual string? DeletedBy { get; set; }
    public virtual DateTime? DeletedDate { get; set; }

    // Concurrency
    [Timestamp]
    public virtual byte[]? RowVersion { get; set; }

    // Domain Events
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
