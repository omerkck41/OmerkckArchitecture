using Core.Persistence.Entities;

namespace Core.Security.Entities;

public class OperationClaim<TId> : Entity<TId>
{
    public string Name { get; set; }

    public OperationClaim() { }

    public OperationClaim(TId id, string name) : base(id)
    {
        Name = name;
    }
}