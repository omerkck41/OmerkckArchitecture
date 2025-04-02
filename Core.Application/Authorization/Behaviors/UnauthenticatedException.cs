using Core.CrossCuttingConcerns.GlobalException.Exceptions;
using System.Net;

namespace Core.Application.Authorization.Behaviors;

public class UnauthenticatedException : CustomException
{
    public UnauthenticatedException(string message = "Authentication required")
        : base(message, new Exception(HttpStatusCode.Unauthorized.ToString()))
    {
    }
}
