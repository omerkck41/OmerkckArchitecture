using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using AwesomeAssertions;
using Xunit;

namespace Kck.FileStorage.AwsS3.Tests;

/// <summary>
/// Which S3 errors of a conditional upload (<c>Overwrite = false</c>) mean "the object exists". The 412 path is also
/// proven against S3Mock; the 409 race cannot be provoked by a container test, and the service builds its own client,
/// so the classification is tested directly.
/// </summary>
public sealed class AwsS3ConditionalWriteTests
{
    private static AmazonS3Exception Error(HttpStatusCode status, string errorCode) =>
        new("test", ErrorType.Sender, errorCode, "request-id", status);

    [Theory]
    [InlineData(HttpStatusCode.PreconditionFailed, "PreconditionFailed")]
    [InlineData(HttpStatusCode.Conflict, "ConditionalRequestConflict")]
    public void IsExistingObjectConflict_PreconditionFailedOrConditionalConflict_ReturnsTrue(
        HttpStatusCode status, string errorCode)
    {
        AwsS3StorageService.IsExistingObjectConflict(Error(status, errorCode)).Should().BeTrue();
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "OperationAborted")]
    [InlineData(HttpStatusCode.Forbidden, "AccessDenied")]
    [InlineData(HttpStatusCode.NotFound, "NoSuchBucket")]
    public void IsExistingObjectConflict_OtherErrors_ReturnsFalse(HttpStatusCode status, string errorCode)
    {
        AwsS3StorageService.IsExistingObjectConflict(Error(status, errorCode)).Should().BeFalse();
    }
}
