namespace Core.Persistence.Entities;

public abstract class Entity<TId> : IEntity<TId>
{
    public Entity() { Id = default!; }

    public Entity(TId id)
    {
        Id = id;
    }


    public virtual TId Id { get; set; }
    public virtual string CreatedBy { get; set; } = string.Empty;
    public virtual string? ModifiedBy { get; set; }
    public virtual bool IsDeleted { get; set; } = false;
    public virtual string? DeletedBy { get; set; }
    public virtual DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public virtual DateTime? ModifiedDate { get; set; }
    public virtual DateTime? DeletedDate { get; set; }
}