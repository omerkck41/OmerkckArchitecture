using MediatR;

namespace Core.Application.Authorization.Behaviors;

public class GetOrdersQuery : IRequest<List<string>>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Admin, GeneralOperationClaims.Manager];
    public Dictionary<string, string> Claims => new() { { "Permission", "ViewOrders" } };
}