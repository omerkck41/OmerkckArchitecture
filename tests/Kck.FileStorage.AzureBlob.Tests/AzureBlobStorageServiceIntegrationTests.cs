using Azure;
using AwesomeAssertions;
using Kck.FileStorage.Abstractions;
using Kck.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.FileStorage.AzureBlob.Tests;

/// <summary>
/// Behaviour of <see cref="AzureBlobStorageService"/> against Azurite, resolved through
/// <c>AddKckFileStorageAzureBlob</c>. Every test works in its own blob container, except the K7 test, which owns the
/// containers <c>alpha</c> and <c>beta</c>.
/// </summary>
/// <remarks>Requires Docker. CI: ubuntu-only (Category=Integration filter).</remarks>
[Trait("Category", "Integration")]
public sealed class AzureBlobStorageServiceIntegrationTests(AzuriteContainerFixture azurite)
    : IClassFixture<AzuriteContainerFixture>
{
    [DockerFact]
    public async Task UploadThenDownload_RoundTripsContent()
    {
        await using var provider = azurite.BuildProvider(AzuriteContainerFixture.UniqueContainerName());
        var sut = provider.GetRequiredService<IFileStorageService>();

        await sut.UploadTextAsync("reports/2026", "q1.txt", "quarter one");
        var text = await sut.DownloadTextAsync("reports/2026/q1.txt");

        text.Should().Be("quarter one");
    }

    [DockerFact]
    public async Task Exists_AfterUpload_True_AfterDelete_False()
    {
        await using var provider = azurite.BuildProvider(AzuriteContainerFixture.UniqueContainerName());
        var sut = provider.GetRequiredService<IFileStorageService>();

        await sut.UploadTextAsync("docs", "a.txt", "a");
        var afterUpload = await sut.ExistsAsync("docs/a.txt");
        await sut.DeleteAsync("docs/a.txt");
        var afterDelete = await sut.ExistsAsync("docs/a.txt");

        afterUpload.Should().BeTrue();
        afterDelete.Should().BeFalse();
    }

    [DockerFact]
    public async Task Upload_SamePathTwice_WithOverwrite_ReplacesContent()
    {
        await using var provider = azurite.BuildProvider(AzuriteContainerFixture.UniqueContainerName());
        var sut = provider.GetRequiredService<IFileStorageService>();

        await sut.UploadTextAsync("docs", "a.txt", "first");
        await sut.UploadTextAsync("docs", "a.txt", "second", overwrite: true);

        (await sut.DownloadTextAsync("docs/a.txt")).Should().Be("second");
    }

    [DockerFact]
    public async Task Upload_ExistingPath_WithoutOverwrite_ThrowsIOExceptionAndKeepsContent()
    {
        await using var provider = azurite.BuildProvider(AzuriteContainerFixture.UniqueContainerName());
        var sut = provider.GetRequiredService<IFileStorageService>();
        await sut.UploadTextAsync("docs", "a.txt", "first");

        var act = () => sut.UploadTextAsync("docs", "a.txt", "second", overwrite: false);

        var inner = (await act.Should().ThrowAsync<IOException>())
            .WithInnerException<RequestFailedException>().Which;
        inner.Status.Should().Be(409);
        inner.ErrorCode.Should().Be("BlobAlreadyExists");
        (await sut.DownloadTextAsync("docs/a.txt")).Should().Be("first");
    }

    [DockerFact]
    public async Task List_WithPrefix_ReturnsOnlyMatchingEntriesWithMetadata()
    {
        await using var provider = azurite.BuildProvider(AzuriteContainerFixture.UniqueContainerName());
        var sut = provider.GetRequiredService<IFileStorageService>();
        await sut.UploadTextAsync("reports", "a.txt", "12345");
        await sut.UploadTextAsync("reports", "b.txt", "123");
        await sut.UploadTextAsync("reports-archive", "old.txt", "x");
        await sut.UploadTextAsync("other", "c.txt", "x");

        var entries = await sut.ListAsync("reports");

        entries.Select(e => e.FullPath).Should().BeEquivalentTo(["reports/a.txt", "reports/b.txt"]);
        var a = entries.Single(e => e.FileName == "a.txt");
        a.SizeInBytes.Should().Be(5);
        a.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));
        a.ModifiedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));
    }

    [DockerFact]
    public async Task List_EmptyDirectory_ReturnsEmptyList()
    {
        await using var provider = azurite.BuildProvider(AzuriteContainerFixture.UniqueContainerName());
        var sut = provider.GetRequiredService<IFileStorageService>();

        var entries = await sut.ListAsync("nothing-here");

        entries.Should().BeEmpty();
    }

    // The listing is recursive (a flat blob listing): the root lists nested blobs too.
    [DockerFact]
    public async Task List_Root_ReturnsEveryEntryRecursively()
    {
        await using var provider = azurite.BuildProvider(AzuriteContainerFixture.UniqueContainerName());
        var sut = provider.GetRequiredService<IFileStorageService>();
        await sut.UploadTextAsync("", "root.txt", "x");
        await sut.UploadTextAsync("docs", "nested.txt", "x");

        var entries = await sut.ListAsync("");

        entries.Select(e => e.FullPath).Should().BeEquivalentTo(["root.txt", "docs/nested.txt"]);
    }

    [DockerFact]
    public async Task Download_Missing_ThrowsNotFoundFromTheSdk()
    {
        await using var provider = azurite.BuildProvider(AzuriteContainerFixture.UniqueContainerName());
        var sut = provider.GetRequiredService<IFileStorageService>();

        var act = () => sut.DownloadAsync("docs/missing.txt");

        (await act.Should().ThrowAsync<RequestFailedException>()).Which.Status.Should().Be(404);
    }

    [DockerFact]
    public async Task Delete_Missing_DoesNotThrow()
    {
        await using var provider = azurite.BuildProvider(AzuriteContainerFixture.UniqueContainerName());
        var sut = provider.GetRequiredService<IFileStorageService>();

        var act = () => sut.DeleteAsync("docs/missing.txt");

        await act.Should().NotThrowAsync();
    }

    [DockerFact]
    public async Task FirstUse_WithCreateContainerIfNotExists_CreatesContainer()
    {
        var containerName = AzuriteContainerFixture.UniqueContainerName();
        await using var provider = azurite.BuildProvider(containerName, createContainerIfNotExists: true);
        var sut = provider.GetRequiredService<IFileStorageService>();
        var beforeFirstUse = await azurite.ContainerExistsAsync(containerName);

        var exists = await sut.ExistsAsync("docs/a.txt");

        beforeFirstUse.Should().BeFalse("the service is resolved, but the container is created on first use");
        exists.Should().BeFalse();
        (await azurite.ContainerExistsAsync(containerName)).Should().BeTrue();
    }

    [DockerFact]
    public async Task FirstUse_WithCreateContainerIfNotExistsFalse_AndMissingContainer_ThrowsContainerNotFound()
    {
        var containerName = AzuriteContainerFixture.UniqueContainerName();
        await using var provider = azurite.BuildProvider(containerName, createContainerIfNotExists: false);
        var sut = provider.GetRequiredService<IFileStorageService>();

        var act = () => sut.UploadTextAsync("docs", "a.txt", "a");

        (await act.Should().ThrowAsync<RequestFailedException>()).Which.Status.Should().Be(404);
        (await azurite.ContainerExistsAsync(containerName)).Should().BeFalse();
    }

    // K7 (2026-10-31 health analysis, probe P6): bound to "alpha", the service used to read beta's "secret.txt" for
    // "../beta/secret.txt" and to write "pwned.txt" into beta for DestinationPath "../beta".
    [DockerFact]
    public async Task EscapingPath_FromContainerAlpha_CannotReachOrChangeContainerBeta()
    {
        var beta = azurite.Raw.GetBlobContainerClient("beta");
        await beta.CreateIfNotExistsAsync();
        await beta.GetBlobClient("secret.txt").UploadAsync(BinaryData.FromString("beta's secret"), overwrite: true);
        var betaBefore = await azurite.ListBlobNamesAsync("beta");
        await using var provider = azurite.BuildProvider("alpha");
        var sut = provider.GetRequiredService<IFileStorageService>();

        var exists = () => sut.ExistsAsync("../beta/secret.txt");
        var download = () => sut.DownloadAsync("../beta/secret.txt");
        var upload = () => sut.UploadTextAsync("../beta", "pwned.txt", "pwned");

        await exists.Should().ThrowAsync<ArgumentException>();
        await download.Should().ThrowAsync<ArgumentException>();
        await upload.Should().ThrowAsync<ArgumentException>();
        // The service creates "alpha" on its first request, after path validation: alpha still missing means none of
        // the three calls reached Azurite.
        (await azurite.ContainerExistsAsync("alpha")).Should().BeFalse();
        betaBefore.Should().Equal("secret.txt");
        (await azurite.ListBlobNamesAsync("beta")).Should().Equal(betaBefore);
        (await beta.GetBlobClient("secret.txt").DownloadContentAsync()).Value.Content.ToString()
            .Should().Be("beta's secret");
    }
}
