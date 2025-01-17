using Core.Persistence.Entities;

namespace Core.Security.Entities;

public class UserOperationClaim<TId, TUserId, TOperationClaimId> : Entity<TId>
{
    public TUserId UserId { get; set; }
    public TOperationClaimId OperationClaimId { get; set; }

    public virtual User<TId> User { get; set; }
    public virtual OperationClaim<TId> OperationClaim { get; set; }

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