namespace Core.Application.Authorization.Behaviors;

public class AuthorizationException : Exception
{
    public AuthorizationException(string message) : base(message)
    {
    }
}