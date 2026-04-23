namespace Kck.Core.Abstractions.Entities;

public interface IAuditable
{
    string CreatedBy { get; set; }
    DateTime CreatedDate { get; set; }
    string? ModifiedBy { get; set; }
    DateTime? ModifiedDate { get; set; }
}
