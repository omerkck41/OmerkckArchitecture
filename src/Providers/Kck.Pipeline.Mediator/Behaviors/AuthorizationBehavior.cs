using Kck.Authorization.Abstractions;
using Kck.Core.Abstractions.Pipeline;
using Kck.Exceptions;
using Mediator;

namespace Kck.Pipeline.Mediator.Behaviors;

/// <summary>
/// Enforces role-based authorization for requests implementing <see cref="ISecuredRequest"/>.
/// </summary>
public sealed class AuthorizationBehavior<TMessage, TResponse>(
    ICurrentUserProvider currentUser)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IRequest<TResponse>, ISecuredRequest
{
    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
            throw new UnauthorizedException();

        if (message.Roles.Length > 0 && !message.Roles.Any(r => currentUser.IsInRole(r)))
            throw new ForbiddenException($"Required roles: {string.Join(", ", message.Roles)}");

        return await next(message, cancellationToken);
    }
}
