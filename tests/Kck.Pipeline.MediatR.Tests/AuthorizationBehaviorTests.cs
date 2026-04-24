using FluentAssertions;
using Kck.Authorization.Abstractions;
using Kck.Core.Abstractions.Pipeline;
using Kck.Exceptions;
using Kck.Pipeline.MediatR.Behaviors;
using MediatR;
using NSubstitute;
using Xunit;

namespace Kck.Pipeline.MediatR.Tests;

public class AuthorizationBehaviorTests
{
    private sealed record TestSecuredRequest(string[] Roles) : IRequest<string>, ISecuredRequest;

    private readonly ICurrentUserProvider _currentUser = Substitute.For<ICurrentUserProvider>();

    [Fact]
    public async Task Handle_UnauthenticatedUser_ShouldThrowUnauthorized()
    {
        // Arrange
        _currentUser.IsAuthenticated.Returns(false);
        var behavior = new AuthorizationBehavior<TestSecuredRequest, string>(_currentUser);
        var request = new TestSecuredRequest(["Admin"]);

        // Act
        var act = () => behavior.Handle(request, _ => Task.FromResult("ok"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Handle_MissingRole_ShouldThrowForbidden()
    {
        // Arrange
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsInRole("Admin").Returns(false);

        var behavior = new AuthorizationBehavior<TestSecuredRequest, string>(_currentUser);
        var request = new TestSecuredRequest(["Admin"]);

        // Act
        var act = () => behavior.Handle(request, _ => Task.FromResult("ok"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_ValidRole_ShouldCallNext()
    {
        // Arrange
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsInRole("Admin").Returns(true);

        var behavior = new AuthorizationBehavior<TestSecuredRequest, string>(_currentUser);
        var request = new TestSecuredRequest(["Admin"]);
        var nextCalled = false;

        // Act
        var result = await behavior.Handle(request, _ =>
        {
            nextCalled = true;
            return Task.FromResult("ok");
        }, CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NoRolesRequired_ShouldCallNext()
    {
        // Arrange
        _currentUser.IsAuthenticated.Returns(true);
        var behavior = new AuthorizationBehavior<TestSecuredRequest, string>(_currentUser);
        var request = new TestSecuredRequest([]);

        // Act
        var result = await behavior.Handle(request, _ => Task.FromResult("ok"), CancellationToken.None);

        // Assert
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_HasOneOfMultipleRoles_ShouldCallNext()
    {
        // Arrange
        _currentUser.IsAuthenticated.Returns(true);
        _currentUser.IsInRole("Admin").Returns(false);
        _currentUser.IsInRole("Editor").Returns(true);

        var behavior = new AuthorizationBehavior<TestSecuredRequest, string>(_currentUser);
        var request = new TestSecuredRequest(["Admin", "Editor"]);

        // Act
        var result = await behavior.Handle(request, _ => Task.FromResult("ok"), CancellationToken.None);

        // Assert
        result.Should().Be("ok");
    }
}
