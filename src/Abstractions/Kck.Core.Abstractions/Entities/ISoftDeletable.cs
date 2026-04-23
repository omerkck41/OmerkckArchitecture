namespace Kck.Core.Abstractions.Entities;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    string? DeletedBy { get; set; }
    DateTime? DeletedDate { get; set; }
}
