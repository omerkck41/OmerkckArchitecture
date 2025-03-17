namespace Core.Application.Authorization.Models;

public interface ISecuredRequest
{
    string[] Roles => Array.Empty<string>();
    Dictionary<string, string> Claims => new();
}