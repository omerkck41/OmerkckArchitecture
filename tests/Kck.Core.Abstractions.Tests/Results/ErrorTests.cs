using AwesomeAssertions;
using Kck.Core.Abstractions.Results;
using Xunit;

namespace Kck.Core.Abstractions.Tests.Results;

public sealed class ErrorTests
{
    [Fact]
    public void Error_DefaultType_IsFailure()
    {
        var error = new Error("ERR", "message");

        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Error_WithExplicitType_SetsType()
    {
        var error = new Error("NOT_FOUND", "not found", ErrorType.NotFound);

        error.Type.Should().Be(ErrorType.NotFound);
        error.Code.Should().Be("NOT_FOUND");
        error.Message.Should().Be("not found");
    }

    [Fact]
    public void Error_Equality_SameCodeAndMessage_AreEqual()
    {
        var a = new Error("CODE", "msg");
        var b = new Error("CODE", "msg");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Error_Equality_DifferentCode_AreNotEqual()
    {
        var a = new Error("CODE_A", "msg");
        var b = new Error("CODE_B", "msg");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Result_Success_IsSuccessTrue()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Result_Failure_IsSuccessFalse()
    {
        var error = new Error("ERR", "fail");
        var result = Result<int>.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().Be(default);
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Result_Match_OnSuccess_CallsSuccessFunc()
    {
        var result = Result<int>.Success(7);

        var output = result.Match(v => $"ok:{v}", e => $"err:{e.Code}");

        output.Should().Be("ok:7");
    }

    [Fact]
    public void Result_Match_OnFailure_CallsFailureFunc()
    {
        var error = new Error("E001", "bad");
        var result = Result<int>.Failure(error);

        var output = result.Match(v => $"ok:{v}", e => $"err:{e.Code}");

        output.Should().Be("err:E001");
    }

    [Fact]
    public void ResultNonGeneric_Success_IsSuccessTrue()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void ResultNonGeneric_Failure_IsSuccessFalse()
    {
        var error = new Error("E002", "fail");
        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Theory]
    [InlineData(ErrorType.Failure)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    public void ErrorType_AllValues_CanBeSetOnError(ErrorType type)
    {
        var error = new Error("CODE", "message", type);

        error.Type.Should().Be(type);
    }
}
