using AwesomeAssertions;
using Kck.FileStorage.Abstractions;
using Kck.Testing;
using Xunit;

namespace Kck.FileStorage.AzureBlob.Tests;

/// <summary>
/// K7 (2026-10-31 health analysis, probe P6 on Azurite): bound to container "alpha", the service returned beta's
/// "secret.txt" for "../beta/secret.txt" and wrote "pwned.txt" into beta for DestinationPath "../beta". The SDK puts
/// the blob name into the request URI and <see cref="Uri"/> collapses the ".." segment. Every path argument is now
/// validated before a request is built: a rejected path never reaches the network.
/// </summary>
public sealed class AzureBlobStorageServicePathTests
{
    // Azurite's well-known development account. Nothing listens on it here, so a request that does reach the
    // network fails with a connection error, not with ArgumentException.
    private static AzureBlobStorageService CreateSut() => new(new StaticOptionsMonitor<AzureBlobOptions>(
        new AzureBlobOptions
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ContainerName = "alpha",
            CreateContainerIfNotExists = false
        }));

    [Theory]
    [InlineData("../beta/secret.txt")]
    [InlineData("..\\beta\\secret.txt")]
    [InlineData("/beta/secret.txt")]
    [InlineData("%2e%2e/beta/secret.txt")]
    public async Task ExistsAsync_RejectsAnEscapingPath(string path)
    {
        var act = () => CreateSut().ExistsAsync(path);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DownloadAsync_RejectsAnEscapingPath()
    {
        var act = () => CreateSut().DownloadAsync("../beta/secret.txt");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteAsync_RejectsAnEscapingPath()
    {
        var act = () => CreateSut().DeleteAsync("../beta/secret.txt");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListAsync_RejectsAnEscapingDirectory()
    {
        var act = () => CreateSut().ListAsync("../beta");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UploadAsync_RejectsAnEscapingDestinationPath()
    {
        using var content = new MemoryStream("pwned"u8.ToArray());
        var act = () => CreateSut().UploadAsync(new FileUploadRequest
        {
            Content = content,
            FileName = "pwned.txt",
            DestinationPath = "../beta",
            Overwrite = true
        });

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UploadAsync_RejectsAnEscapingFileName()
    {
        using var content = new MemoryStream("pwned"u8.ToArray());
        var act = () => CreateSut().UploadAsync(new FileUploadRequest
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
        var act = () => CreateSut().ExistsAsync(path, Cancelled);

        await act.Should().NotThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("uploads")]
    public async Task UploadAsync_AcceptsTheRootAndARelativeDestination(string destination)
    {
        using var content = new MemoryStream("ok"u8.ToArray());
        var act = () => CreateSut().UploadAsync(new FileUploadRequest
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
        var act = () => CreateSut().ListAsync(directory, Cancelled);

        await act.Should().NotThrowAsync<ArgumentException>();
    }
}
