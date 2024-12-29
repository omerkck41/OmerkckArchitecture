using Core.Security.Entities;

namespace Core.Security.JWT;

public interface ITokenHelper
{
    Task<AccessToken> CreateTokenAsync(User user, IList<OperationClaim> operationClaims);
    Task<RefreshToken> CreateRefreshTokenAsync(User user, string ipAddress);
}