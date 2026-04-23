namespace Kck.Core.Abstractions.Pipeline;

public interface ISecuredRequest
{
    string[] Roles { get; }
}
