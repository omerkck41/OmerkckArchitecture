namespace Core.Application.Authorization.Models;

public interface ISecuredRequest
{
    string[] Roles => [];

    Dictionary<string, string> Claims => [];
}