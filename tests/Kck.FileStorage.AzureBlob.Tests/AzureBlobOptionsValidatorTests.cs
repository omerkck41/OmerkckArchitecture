using FluentAssertions;
using Kck.FileStorage.AzureBlob;
using Kck.FileStorage.AzureBlob.DependencyInjection;
using Xunit;

namespace Kck.FileStorage.AzureBlob.Tests;

public sealed class AzureBlobOptionsValidatorTests
{
    private readonly AzureBlobOptionsValidator _sut = new();

    [Fact]
    public void Validate_ConnectionString_ReturnsSuccess()
    {
        var opts = new AzureBlobOptions { ConnectionString = "DefaultEndpointsProtocol=https;...", ContainerName = "files" };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_AccountName_ReturnsSuccess()
    {
        var opts = new AzureBlobOptions { AccountName = "myaccount", ContainerName = "files" };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_NeitherAuthSet_ReturnsFail()
    {
        var opts = new AzureBlobOptions { ContainerName = "files" };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ConnectionString").And.Contain("AccountName");
    }

    [Fact]
    public void Validate_BothAuthSet_ReturnsFail()
    {
        var opts = new AzureBlobOptions
        {
            ConnectionString = "DefaultEndpoints...",
            AccountName = "myaccount",
            ContainerName = "files"
        };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public void Validate_EmptyContainerName_ReturnsFail()
    {
        var opts = new AzureBlobOptions { ConnectionString = "DefaultEndpoints...", ContainerName = "" };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ContainerName");
    }
}
