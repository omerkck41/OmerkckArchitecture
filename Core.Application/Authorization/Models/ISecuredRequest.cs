namespace Core.Application.Authorization.Models;

public interface ISecuredRequest
{
    public string[] Roles { get; }
    Dictionary<string, string> Claims { get; } // Key: ClaimType, Value: ClaimValue
}