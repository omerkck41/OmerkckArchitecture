namespace Core.Application.Authorization.Behaviors;

public interface ISecuredRequest
{
    string[] Roles { get; }
    Dictionary<string, string> Claims { get; } // Key: ClaimType, Value: ClaimValue
}