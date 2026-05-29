using System.Globalization;
using AwesomeAssertions;
using Kck.Core.Abstractions.Results;
using Xunit;

namespace Kck.Core.Abstractions.Tests.Results;

public sealed class ResultExtensionsTests
{
    private static readonly Error TestError = new("TEST_ERR", "test error");

    // --- Map ---

    [Fact]
    public void Map_SuccessResult_ShouldTransformValue()
    {
        var result = Result<int>.Success(5);
        var mapped = result.Map(x => x * 2);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_FailureResult_ShouldPropagateError()
    {
        var result = Result<int>.Failure(TestError);
        var mapped = result.Map(x => x * 2);
        mapped.IsSuccess.Should().BeFalse();
        mapped.Error.Should().Be(TestError);
    }

    // --- Bind ---

    [Fact]
    public void Bind_SuccessResult_ShouldChainNextOperation()
    {
        var result = Result<int>.Success(5);
        var bound = result.Bind(x => Result<string>.Success(x.ToString(CultureInfo.InvariantCulture)));
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_SuccessResult_WhenBinderFails_ShouldReturnFailure()
    {
        var result = Result<int>.Success(5);
        var bound = result.Bind(_ => Result<string>.Failure(TestError));
        bound.IsSuccess.Should().BeFalse();
        bound.Error.Should().Be(TestError);
    }

    [Fact]
    public void Bind_FailureResult_ShouldNotCallBinder()
    {
        var binderCalled = false;
        var result = Result<int>.Failure(TestError);
        var bound = result.Bind(x => { binderCalled = true; return Result<string>.Success(x.ToString(CultureInfo.InvariantCulture)); });
        bound.IsSuccess.Should().BeFalse();
        binderCalled.Should().BeFalse();
    }

    // --- Tap ---

    [Fact]
    public void Tap_SuccessResult_ShouldExecuteActionAndReturnSameResult()
    {
        var executed = false;
        var result = Result<int>.Success(42);
        var tapped = result.Tap(_ => executed = true);
        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(42);
        executed.Should().BeTrue();
    }

    [Fact]
    public void Tap_FailureResult_ShouldNotExecuteAction()
    {
        var executed = false;
        var result = Result<int>.Failure(TestError);
        var tapped = result.Tap(_ => executed = true);
        tapped.IsSuccess.Should().BeFalse();
        executed.Should().BeFalse();
    }

    // --- Ensure ---

    [Fact]
    public void Ensure_SuccessResult_PredicatePasses_ShouldRemainSuccess()
    {
        var result = Result<int>.Success(10);
        var ensured = result.Ensure(x => x > 5, TestError);
        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(10);
    }

    [Fact]
    public void Ensure_SuccessResult_PredicateFails_ShouldReturnFailure()
    {
        var result = Result<int>.Success(3);
        var ensured = result.Ensure(x => x > 5, TestError);
        ensured.IsSuccess.Should().BeFalse();
        ensured.Error.Should().Be(TestError);
    }

    [Fact]
    public void Ensure_FailureResult_ShouldPropagateOriginalError()
    {
        var differentError = new Error("OTHER", "other error");
        var result = Result<int>.Failure(TestError);
        var ensured = result.Ensure(x => x > 5, differentError);
        ensured.IsSuccess.Should().BeFalse();
        ensured.Error.Should().Be(TestError, "original error should propagate, not the ensure error");
    }

    // --- Null guards ---

    [Fact]
    public void Map_NullMapper_ThrowsArgumentNullException()
    {
        var result = Result<int>.Success(5);
        var act = () => result.Map<int, string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Bind_NullBinder_ThrowsArgumentNullException()
    {
        var result = Result<int>.Success(5);
        var act = () => result.Bind<int, string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Tap_NullAction_ThrowsArgumentNullException()
    {
        var result = Result<int>.Success(5);
        var act = () => result.Tap(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ensure_NullPredicate_ThrowsArgumentNullException()
    {
        var result = Result<int>.Success(5);
        var act = () => result.Ensure(null!, TestError);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ensure_NullError_ThrowsArgumentNullException()
    {
        var result = Result<int>.Success(5);
        var act = () => result.Ensure(x => x > 0, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // --- Pipeline ---

    [Fact]
    public void Pipeline_ChainedOperations_ShouldWorkTogether()
    {
        var sideEffectValue = 0;
        var result = Result<int>.Success(5)
            .Ensure(x => x > 0, new Error("NEG", "must be positive"))
            .Map(x => x * 2)
            .Tap(x => sideEffectValue = x)
            .Bind(x => x > 5
                ? Result<string>.Success($"large:{x}")
                : Result<string>.Failure(new Error("SMALL", "too small")));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("large:10");
        sideEffectValue.Should().Be(10);
    }
}
