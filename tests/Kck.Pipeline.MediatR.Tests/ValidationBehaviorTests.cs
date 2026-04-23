using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Kck.Pipeline.MediatR.Behaviors;
using MediatR;
using NSubstitute;
using Xunit;
using ValidationException = Kck.Exceptions.ValidationException;

namespace Kck.Pipeline.MediatR.Tests;

public record ValidatableRequest(string Name) : IRequest<string>;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<ValidatableRequest>>();
        var behavior = new ValidationBehavior<ValidatableRequest, string>(validators);
        var request = new ValidatableRequest("test");
        var nextCalled = false;

        Task<string> Next(CancellationToken _)
        {
            nextCalled = true;
            return Task.FromResult("ok");
        }

        // Act
        var result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        result.Should().Be("ok");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithPassingValidation_ShouldCallNext()
    {
        // Arrange
        var validator = Substitute.For<IValidator<ValidatableRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<ValidatableRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<ValidatableRequest, string>([validator]);
        var request = new ValidatableRequest("valid");

        // Act
        var result = await behavior.Handle(request, _ => Task.FromResult("ok"), CancellationToken.None);

        // Assert
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WithValidationErrors_ShouldThrowValidationException()
    {
        // Arrange
        var validator = Substitute.For<IValidator<ValidatableRequest>>();
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Name", "Name must be at least 3 characters")
        };
        validator.ValidateAsync(Arg.Any<ValidationContext<ValidatableRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var behavior = new ValidationBehavior<ValidatableRequest, string>([validator]);
        var request = new ValidatableRequest("");

        // Act
        var act = () => behavior.Handle(request, _ => Task.FromResult("ok"), CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(1);
        exception.Which.Errors.First().Property.Should().Be("Name");
        exception.Which.Errors.First().Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldAggregateErrors()
    {
        // Arrange
        var validator1 = Substitute.For<IValidator<ValidatableRequest>>();
        validator1.ValidateAsync(Arg.Any<ValidationContext<ValidatableRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Too short")]));

        var validator2 = Substitute.For<IValidator<ValidatableRequest>>();
        validator2.ValidateAsync(Arg.Any<ValidationContext<ValidatableRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Email", "Required")]));

        var behavior = new ValidationBehavior<ValidatableRequest, string>([validator1, validator2]);

        // Act
        var act = () => behavior.Handle(new ValidatableRequest(""), _ => Task.FromResult("ok"), CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithValidationErrors_ShouldNotCallNext()
    {
        // Arrange
        var validator = Substitute.For<IValidator<ValidatableRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<ValidatableRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult([new ValidationFailure("Name", "Required")]));

        var behavior = new ValidationBehavior<ValidatableRequest, string>([validator]);
        var nextCalled = false;

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new ValidatableRequest(""), _ =>
            {
                nextCalled = true;
                return Task.FromResult("ok");
            }, CancellationToken.None));

        nextCalled.Should().BeFalse();
    }
}
