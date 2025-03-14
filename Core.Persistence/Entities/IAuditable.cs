namespace Core.Persistence.Entities;

public interface IAuditable
{
    DateTime CreatedDate { get; set; }
    DateTime? ModifiedDate { get; set; }
    bool IsDeleted { get; set; }
    string CreatedBy { get; set; }
    string? ModifiedBy { get; set; }
    DateTime? DeletedDate { get; set; }
    string? DeletedBy { get; set; }
}