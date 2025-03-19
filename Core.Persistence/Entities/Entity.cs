using System.ComponentModel.DataAnnotations;

namespace Core.Persistence.Entities;

public abstract class Entity<TId> : IEntity<TId>, IAuditable
{
    public Entity() { Id = default!; }

    public Entity(TId id)
    {
        Id = id;
    }


    public virtual TId Id { get; set; }
    public virtual string CreatedBy { get; set; } = string.Empty;
    public virtual DateTime CreatedDate { get; set; } = DateTime.UtcNow;


    public virtual string? ModifiedBy { get; set; }
    public virtual bool IsDeleted { get; set; } = false;
    public virtual string? DeletedBy { get; set; }
    public virtual DateTime? ModifiedDate { get; set; }
    public virtual DateTime? DeletedDate { get; set; }


    // Concurrency token: EF Core, bu alanı otomatik olarak güncelleyecektir.
    [Timestamp]
    public virtual byte[]? RowVersion { get; set; }


    // Explicit interface implementasyonu:
    string IAuditable.CreatedBy
    {
        get => CreatedBy;
        set => SetCreatedBy(value);
    }

    DateTime IAuditable.CreatedDate
    {
        get => CreatedDate;
        set => SetCreatedDate(value);
    }

    // İsteğe bağlı: CreatedBy ve CreatedDate'in yalnızca ilk atamada set edilmesini sağlayan yardımcı metotlar:
    protected void SetCreatedBy(string value)
    {
        if (string.IsNullOrEmpty(CreatedBy))
        {
            CreatedBy = value;
        }
    }

    protected void SetCreatedDate(DateTime value)
    {
        if (CreatedDate == default)
        {
            CreatedDate = value;
        }
    }
}