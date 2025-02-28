using Core.Persistence.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Security.Entities;

public class OperationClaim<TId> : Entity<TId>
{
    public string Name { get; set; }

    [NotMapped]
    public virtual ICollection<UserOperationClaim<TId, TId, TId>> UserOperationClaims { get; set; }

    public OperationClaim()
    {
        UserOperationClaims = new List<UserOperationClaim<TId, TId, TId>>();
        Name = string.Empty;
    }

    public OperationClaim(string name)
    {
        Name = name;
    }

    public OperationClaim(TId id, string name) : base(id)
    {
        Name = name;
    }
}