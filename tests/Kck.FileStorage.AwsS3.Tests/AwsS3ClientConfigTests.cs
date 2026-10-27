using Amazon;
using Amazon.Runtime;
using AwesomeAssertions;
using Kck.FileStorage.AwsS3;
using Xunit;

namespace Kck.FileStorage.AwsS3.Tests;

public sealed class AwsS3ClientConfigTests
{
    [Fact]
    public void CreateClientConfig_RegionOnly_UsesAwsRegionEndpointAndSdkDefaults()
    {
        var config = AwsS3StorageService.CreateClientConfig(
            new AwsS3Options { Region = "eu-central-1", BucketName = "b" });

        config.RegionEndpoint.Should().Be(RegionEndpoint.EUCentral1);
        config.ServiceURL.Should().BeNull();
        config.ForcePathStyle.Should().BeFalse();
        config.RequestChecksumCalculation.Should().Be(RequestChecksumCalculation.WHEN_SUPPORTED);
        config.ResponseChecksumValidation.Should().Be(ResponseChecksumValidation.WHEN_SUPPORTED);
    }

    [Fact]
    public void CreateClientConfig_ServiceUrl_TargetsCustomEndpointAndSignsWithRegion()
    {
        var config = AwsS3StorageService.CreateClientConfig(new AwsS3Options
        {
            Region = "us-east-1",
            BucketName = "b",
            ServiceUrl = "http://localhost:9000",
            ForcePathStyle = true
        });

        config.ServiceURL.Should().StartWith("http://localhost:9000");
        config.AuthenticationRegion.Should().Be("us-east-1");
        config.ForcePathStyle.Should().BeTrue();
    }

    [Fact]
    public void CreateClientConfig_ChecksumWhenRequired_OnlyComputesRequiredChecksums()
    {
        var config = AwsS3StorageService.CreateClientConfig(new AwsS3Options
        {
            Region = "us-east-1",
            BucketName = "b",
            ChecksumWhenRequired = true
        });

        config.RequestChecksumCalculation.Should().Be(RequestChecksumCalculation.WHEN_REQUIRED);
        config.ResponseChecksumValidation.Should().Be(ResponseChecksumValidation.WHEN_REQUIRED);
    }
}
