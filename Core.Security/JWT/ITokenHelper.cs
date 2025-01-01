using Core.Security.Entities;

namespace Core.Security.JWT;

public interface ITokenHelper<TUserId, TOperationClaimId, TRefreshTokenId>
{
    Task<AccessToken> CreateTokenAsync(User<TUserId> user, IList<OperationClaim<TOperationClaimId>> operationClaims);
    Task<RefreshToken<TRefreshTokenId, TUserId>> CreateRefreshTokenAsync(User<TUserId> user, string ipAddress);
}