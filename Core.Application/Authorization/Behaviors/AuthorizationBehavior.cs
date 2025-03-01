using Core.Application.Authorization.Models;
using Core.Application.Authorization.Services;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Core.Application.Authorization.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ISecuredRequest
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationBehavior(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        AuthorizationValidator.ValidateAuthorization(user, request.Roles, request.Claims);

        return await next();
    }
}