namespace Core.Persistence.Entities;

public abstract class Entity
{
    public Entity() { }

    public Entity(int id) : this()
    {
        Id = id;
    }


    public virtual int Id { get; set; }
    public virtual string CreatedBy { get; set; } = string.Empty;
    public virtual string? ModifiedBy { get; set; }
    public virtual bool IsDeleted { get; set; } = false;
    public virtual string? DeletedBy { get; set; }
    public virtual DateTime CreatedDate { get; set; } = DateTime.Now;
    public virtual DateTime? ModifiedDate { get; set; }
    public virtual DateTime? DeletedDate { get; set; }
}