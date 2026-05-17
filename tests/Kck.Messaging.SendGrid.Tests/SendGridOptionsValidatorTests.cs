using FluentAssertions;
using Kck.Messaging.SendGrid.DependencyInjection;
using Xunit;

namespace Kck.Messaging.SendGrid.Tests;

public sealed class SendGridOptionsValidatorTests
{
    private readonly SendGridOptionsValidator _sut = new();

    [Fact]
    public void Validate_ValidApiKey_ReturnsSuccess()
    {
        var opts = new SendGridOptions { ApiKey = "SG.abc123" };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyApiKey_ReturnsFail(string apiKey)
    {
        var opts = new SendGridOptions { ApiKey = apiKey };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ApiKey");
    }
}
