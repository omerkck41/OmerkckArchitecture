using FluentAssertions;
using Kck.Authorization.Abstractions;
using Kck.Core.Abstractions.Pipeline;
using Kck.Exceptions;
using Kck.Pipeline.Mediator.Behaviors;
using Mediator;
using NSubstitute;
using Xunit;

namespace Kck.Pipeline.Mediator.Tests;

public sealed class AuthorizationBehaviorTests
{
    private sealed record SecuredMessage(string[] RequiredRoles) : IRequest<string>, ISecuredRequest
    {
        public string[] Roles => RequiredRoles;
    }

    [Fact]
    public async Task Handle_UnauthenticatedUser_ShouldThrowUnauthorized()
    {
        var user = Substitute.For<ICurrentUserProvider>();
        user.IsAuthenticated.Returns(false);
        var sut = new AuthorizationBehavior<SecuredMessage, string>(user);

        var act = () => sut.Handle(new SecuredMessage([]), (_, _) => ValueTask.FromResult("ok"), CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_MissingRole_ShouldThrowForbidden()
    {
        var user = Substitute.For<ICurrentUserProvider>();
        user.IsAuthenticated.Returns(true);
        user.IsInRole("Admin").Returns(false);
        var sut = new AuthorizationBehavior<SecuredMessage, string>(user);

        var act = () => sut.Handle(new SecuredMessage(["Admin"]), (_, _) => ValueTask.FromResult("ok"), CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_HasRequiredRole_ShouldCallNext()
    {
        var user = Substitute.For<ICurrentUserProvider>();
        user.IsAuthenticated.Returns(true);
        user.IsInRole("Admin").Returns(true);
        var sut = new AuthorizationBehavior<SecuredMessage, string>(user);

        var result = await sut.Handle(new SecuredMessage(["Admin"]), (_, _) => ValueTask.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_NoRolesRequired_ShouldCallNext()
    {
        var user = Substitute.For<ICurrentUserProvider>();
        user.IsAuthenticated.Returns(true);
        var sut = new AuthorizationBehavior<SecuredMessage, string>(user);

        var result = await sut.Handle(new SecuredMessage([]), (_, _) => ValueTask.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_HasOneOfMultipleRoles_ShouldCallNext()
    {
        var user = Substitute.For<ICurrentUserProvider>();
        user.IsAuthenticated.Returns(true);
        user.IsInRole("Admin").Returns(false);
        user.IsInRole("Editor").Returns(true);
        var sut = new AuthorizationBehavior<SecuredMessage, string>(user);

        var result = await sut.Handle(new SecuredMessage(["Admin", "Editor"]), (_, _) => ValueTask.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }
}
