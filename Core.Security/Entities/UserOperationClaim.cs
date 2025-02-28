using Core.Persistence.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Security.Entities;

public class UserOperationClaim<TId, TUserId, TOperationClaimId> : Entity<TId>
{
    public TUserId UserId { get; set; }
    public TOperationClaimId OperationClaimId { get; set; }

    [NotMapped]
    public virtual User<TUserId> User { get; set; }
    [NotMapped]
    public virtual OperationClaim<TOperationClaimId> OperationClaim { get; set; }

    public UserOperationClaim()
    {
        UserId = default!;
        OperationClaimId = default!;
    }

    public UserOperationClaim(TUserId userId, TOperationClaimId operationClaimId)
    {
        UserId = userId;
        OperationClaimId = operationClaimId;
    }

    public UserOperationClaim(TId id, TUserId userId, TOperationClaimId operationClaimId) : base(id)
    {
        UserId = userId;
        OperationClaimId = operationClaimId;
    }
}