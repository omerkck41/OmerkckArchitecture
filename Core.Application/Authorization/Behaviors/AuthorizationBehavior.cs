using Core.Application.Authorization.Models;
using Core.Security.Extensions;
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

        if (user == null || !user.Identity!.IsAuthenticated)
        {
            throw new AuthorizationException("You are not authenticated.");
        }

        // Role-based authorization
        var userRoles = user.GetRoles();
        if (!userRoles.Any(role => request.Roles.Contains(role) || role == GeneralOperationClaims.Admin))
        {
            throw new AuthorizationException("You are not authorized.");
        }

        // Claim-based authorization
        foreach (var claim in request.Claims)
        {
            if (!user.HasClaim(c => c.Type == claim.Key && c.Value == claim.Value))
            {
                throw new AuthorizationException($"Missing required claim: {claim.Key} - {claim.Value}");
            }
        }

        return await next();
    }
}