using Kck.Authorization.Abstractions;
using Kck.Core.Abstractions.Pipeline;
using Kck.Exceptions;
using MediatR;

namespace Kck.Pipeline.MediatR.Behaviors;

/// <summary>
/// Enforces role-based authorization for requests implementing <see cref="ISecuredRequest"/>.
/// Throws <see cref="UnauthorizedException"/> if the user is not authenticated,
/// or <see cref="ForbiddenException"/> if the user lacks the required roles.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUserProvider currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ISecuredRequest
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException();

        var requiredRoles = request.Roles;
        if (requiredRoles.Length > 0)
        {
            var hasAnyRole = requiredRoles.Any(role => currentUser.IsInRole(role));
            if (!hasAnyRole)
                throw new ForbiddenException($"Required roles: {string.Join(", ", requiredRoles)}");
        }

        return await next(cancellationToken);
    }
}
