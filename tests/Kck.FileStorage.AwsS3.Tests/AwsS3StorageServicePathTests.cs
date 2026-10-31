using AwesomeAssertions;
using Kck.FileStorage.Abstractions;
using Kck.Testing;
using Xunit;

namespace Kck.FileStorage.AwsS3.Tests;

/// <summary>
/// K7 (2026-10-31 health analysis, probe P7 on S3Mock): the S3 SDK does not collapse "..", so "../tenantB/x" did not
/// leave the KeyPrefix, but the literal key "tenantA/../tenantB/x" was written. Every path argument is now validated
/// with the same rule as the Blob and FTP providers: a rejected path never reaches the network.
/// </summary>
public sealed class AwsS3StorageServicePathTests
{
    // Nothing listens on the discard port, so a request that does reach the network fails with a connection
    // error, not with ArgumentException. S3 needs a credential pair to sign requests; these never leave the process.
    private static AwsS3StorageService CreateSut() => new(new StaticOptionsMonitor<AwsS3Options>(new AwsS3Options
    {
        Region = "us-east-1",
        BucketName = "bucket",
        KeyPrefix = "tenantA",
        ServiceUrl = "http://127.0.0.1:9",
        ForcePathStyle = true,
        AccessKey = Guid.NewGuid().ToString("N"),
        SecretKey = Guid.NewGuid().ToString("N")
    }));

    [Theory]
    [InlineData("../tenantB/secret.txt")]
    [InlineData("..\\tenantB\\secret.txt")]
    [InlineData("/tenantB/secret.txt")]
    [InlineData("%2e%2e/tenantB/secret.txt")]
    public async Task ExistsAsync_RejectsAnEscapingPath(string path)
    {
        using var sut = CreateSut();
        var act = () => sut.ExistsAsync(path);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DownloadAsync_RejectsAnEscapingPath()
    {
        using var sut = CreateSut();
        var act = () => sut.DownloadAsync("../tenantB/secret.txt");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteAsync_RejectsAnEscapingPath()
    {
        using var sut = CreateSut();
        var act = () => sut.DeleteAsync("../tenantB/secret.txt");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListAsync_RejectsAnEscapingDirectory()
    {
        using var sut = CreateSut();
        var act = () => sut.ListAsync("../tenantB");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UploadAsync_RejectsAnEscapingDestinationPath()
    {
        using var sut = CreateSut();
        using var content = new MemoryStream("pwned"u8.ToArray());
        var act = () => sut.UploadAsync(new FileUploadRequest
        {
            Content = content,
            FileName = "pwned.txt",
            DestinationPath = "../tenantB",
            Overwrite = true
        });

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UploadAsync_RejectsAnEscapingFileName()
    {
        using var sut = CreateSut();
        using var content = new MemoryStream("pwned"u8.ToArray());
        var act = () => sut.UploadAsync(new FileUploadRequest
        {
            Content = content,
            FileName = "../pwned.txt",
            DestinationPath = "uploads",
            Overwrite = true
        });

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // A path that passes validation reaches the SDK. With an already-cancelled token the SDK stops before a network
    // call, so these tests are fast; the one outcome they rule out is ArgumentException (Ö17: "ok.txt" still works).
    private static readonly CancellationToken Cancelled = new(canceled: true);

    [Theory]
    [InlineData("ok.txt")]
    [InlineData("uploads/ok.txt")]
    public async Task ExistsAsync_AcceptsARelativePath(string path)
    {
        using var sut = CreateSut();
        var act = () => sut.ExistsAsync(path, Cancelled);

        await act.Should().NotThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("uploads")]
    public async Task UploadAsync_AcceptsTheRootAndARelativeDestination(string destination)
    {
        using var sut = CreateSut();
        using var content = new MemoryStream("ok"u8.ToArray());
        var act = () => sut.UploadAsync(new FileUploadRequest
        {
            Content = content,
            FileName = "ok.txt",
            DestinationPath = destination
        }, Cancelled);

        await act.Should().NotThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("uploads")]
    public async Task ListAsync_AcceptsTheRootAndARelativeDirectory(string directory)
    {
        using var sut = CreateSut();
        var act = () => sut.ListAsync(directory, Cancelled);

        await act.Should().NotThrowAsync<ArgumentException>();
    }
}
