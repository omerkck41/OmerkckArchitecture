using AwesomeAssertions;
using Kck.Core.Abstractions.Results;
using Xunit;

namespace Kck.Core.Abstractions.Tests.Results;

public sealed class ResultTests
{
    private static readonly Error Boom = new("E.Boom", "boom", ErrorType.Conflict);

    [Fact]
    public void Success_NonGeneric_IsSuccessWithoutError()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_NonGeneric_CarriesError()
    {
        var result = Result.Failure(Boom);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeSameAs(Boom);
    }

    [Fact]
    public void Match_NonGenericSuccess_InvokesOnSuccess()
    {
        var output = Result.Success().Match(() => "ok", e => $"err:{e.Code}");

        output.Should().Be("ok");
    }

    [Fact]
    public void Match_NonGenericFailure_InvokesOnFailureWithError()
    {
        var output = Result.Failure(Boom).Match(() => "ok", e => $"err:{e.Code}");

        output.Should().Be("err:E.Boom");
    }

    [Fact]
    public void Success_Generic_ExposesValueAndNoError()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_Generic_ExposesErrorAndDefaultValue()
    {
        var result = Result<int>.Failure(Boom);

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().Be(0);
        result.Error.Should().BeSameAs(Boom);
    }

    [Fact]
    public void Error_DefaultType_IsFailure()
    {
        new Error("c", "m").Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Map_FailureResult_DoesNotInvokeMapper()
    {
        var called = false;

        var mapped = Result<int>.Failure(Boom).Map(v => { called = true; return v; });

        called.Should().BeFalse();
        mapped.Error.Should().BeSameAs(Boom);
    }

    [Fact]
    public void Tap_FailureResult_DoesNotInvokeActionAndReturnsSameResult()
    {
        var called = false;
        var failure = Result<int>.Failure(Boom);

        var tapped = failure.Tap(_ => called = true);

        called.Should().BeFalse();
        tapped.Should().BeSameAs(failure);
    }

    [Fact]
    public void Tap_SuccessResult_InvokesActionWithValue()
    {
        var seen = 0;

        Result<int>.Success(7).Tap(v => seen = v);

        seen.Should().Be(7);
    }

    [Fact]
    public void Bind_FailureResult_DoesNotInvokeBinder()
    {
        var called = false;

        var bound = Result<int>.Failure(Boom).Bind(_ => { called = true; return Result<string>.Success("x"); });

        called.Should().BeFalse();
        bound.Error.Should().BeSameAs(Boom);
    }

    [Fact]
    public void Ensure_FailureResult_DoesNotEvaluatePredicate()
    {
        var called = false;
        var failure = Result<int>.Failure(Boom);

        var ensured = failure.Ensure(_ => { called = true; return true; }, new Error("x", "y"));

        called.Should().BeFalse();
        ensured.Error.Should().BeSameAs(Boom);
    }

    [Fact]
    public void Ensure_PredicateFails_ReturnsSuppliedError()
    {
        var other = new Error("Other", "other");

        var ensured = Result<int>.Success(1).Ensure(v => v > 5, other);

        ensured.IsSuccess.Should().BeFalse();
        ensured.Error.Should().BeSameAs(other);
    }

    [Fact]
    public void Extensions_NullDelegates_ThrowArgumentNull()
    {
        var ok = Result<int>.Success(1);

        ((Action)(() => ok.Map<int, int>(null!))).Should().Throw<ArgumentNullException>();
        ((Action)(() => ok.Bind<int, int>(null!))).Should().Throw<ArgumentNullException>();
        ((Action)(() => ok.Tap(null!))).Should().Throw<ArgumentNullException>();
        ((Action)(() => ok.Ensure(null!, Boom))).Should().Throw<ArgumentNullException>();
        ((Action)(() => ok.Ensure(_ => true, null!))).Should().Throw<ArgumentNullException>();
    }
}
