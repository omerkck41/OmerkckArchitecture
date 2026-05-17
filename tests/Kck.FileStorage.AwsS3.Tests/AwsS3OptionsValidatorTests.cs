using FluentAssertions;
using Kck.FileStorage.AwsS3;
using Kck.FileStorage.AwsS3.DependencyInjection;
using Xunit;

namespace Kck.FileStorage.AwsS3.Tests;

public sealed class AwsS3OptionsValidatorTests
{
    private readonly AwsS3OptionsValidator _sut = new();

    [Fact]
    public void Validate_RegionAndBucket_ReturnsSuccess()
    {
        var opts = new AwsS3Options { Region = "eu-central-1", BucketName = "my-bucket" };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithExplicitCredentials_ReturnsSuccess()
    {
        var opts = new AwsS3Options
        {
            Region = "eu-central-1",
            BucketName = "my-bucket",
            AccessKey = "AKIA...",
            SecretKey = "secret"
        };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_MissingRegion_ReturnsFail()
    {
        var opts = new AwsS3Options { Region = "", BucketName = "my-bucket" };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("Region");
    }

    [Fact]
    public void Validate_MissingBucketName_ReturnsFail()
    {
        var opts = new AwsS3Options { Region = "eu-central-1", BucketName = "" };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("BucketName");
    }

    [Fact]
    public void Validate_OnlyAccessKeySet_ReturnsFail()
    {
        var opts = new AwsS3Options { Region = "eu-central-1", BucketName = "b", AccessKey = "AKIA", SecretKey = null };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("AccessKey").And.Contain("SecretKey");
    }
}
