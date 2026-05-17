using FluentAssertions;
using Kck.Security.Argon2.DependencyInjection;
using Xunit;

namespace Kck.Security.Argon2.Tests;

public sealed class Argon2OptionsValidatorTests
{
    private readonly Argon2OptionsValidator _sut = new();

    [Fact]
    public void Validate_OwaspDefaults_ReturnsSuccess()
    {
        var opts = new Argon2Options(); // default values are OWASP-safe
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_MemoryBelowMinimum_ReturnsFail()
    {
        var opts = new Argon2Options { MemorySize = 8192 };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("MemorySize");
    }

    [Fact]
    public void Validate_ZeroIterations_ReturnsFail()
    {
        var opts = new Argon2Options { Iterations = 0 };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("Iterations");
    }

    [Fact]
    public void Validate_ZeroParallelism_ReturnsFail()
    {
        var opts = new Argon2Options { DegreeOfParallelism = 0 };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("DegreeOfParallelism");
    }

    [Fact]
    public void Validate_HashLengthTooSmall_ReturnsFail()
    {
        var opts = new Argon2Options { HashLength = 8 };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("HashLength");
    }

    [Fact]
    public void Validate_SaltLengthTooSmall_ReturnsFail()
    {
        var opts = new Argon2Options { SaltLength = 8 };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("SaltLength");
    }

    [Fact]
    public void Validate_MultipleViolations_FailureMessageContainsAllFields()
    {
        var opts = new Argon2Options { MemorySize = 1024, Iterations = 0, HashLength = 4 };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should()
            .Contain("MemorySize")
            .And.Contain("Iterations")
            .And.Contain("HashLength");
    }
}
