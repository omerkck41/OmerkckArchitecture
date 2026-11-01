using System.Net;
using Amazon.S3;
using AwesomeAssertions;
using Kck.FileStorage.Abstractions;
using Kck.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.FileStorage.AwsS3.Tests;

/// <summary>
/// Behaviour of <see cref="AwsS3StorageService"/> against an S3 API served over HTTP (Adobe S3Mock), resolved through
/// <c>AddKckFileStorageAwsS3</c>. Every test works in its own bucket.
/// </summary>
/// <remarks>Requires Docker. CI: ubuntu-only (Category=Integration filter).</remarks>
[Trait("Category", "Integration")]
public sealed class AwsS3StorageServiceIntegrationTests(S3MockContainerFixture s3)
    : IClassFixture<S3MockContainerFixture>
{
    [DockerFact]
    public async Task UploadThenDownload_RoundTripsContent()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket);
        var sut = provider.GetRequiredService<IFileStorageService>();

        await sut.UploadTextAsync("reports/2026", "q1.txt", "quarter one");
        var text = await sut.DownloadTextAsync("reports/2026/q1.txt");

        text.Should().Be("quarter one");
    }

    [DockerFact]
    public async Task Exists_AfterUpload_True_AfterDelete_False()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket);
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
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket);
        var sut = provider.GetRequiredService<IFileStorageService>();

        await sut.UploadTextAsync("docs", "a.txt", "first");
        await sut.UploadTextAsync("docs", "a.txt", "second", overwrite: true);

        (await sut.DownloadTextAsync("docs/a.txt")).Should().Be("second");
    }

    [DockerFact]
    public async Task Upload_ExistingPath_WithoutOverwrite_ThrowsIOExceptionAndKeepsContent()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket);
        var sut = provider.GetRequiredService<IFileStorageService>();
        await sut.UploadTextAsync("docs", "a.txt", "first");

        var act = () => sut.UploadTextAsync("docs", "a.txt", "second", overwrite: false);

        // The inner 412 proves the service evaluated If-None-Match: * (a service ignoring the header would overwrite).
        (await act.Should().ThrowAsync<IOException>())
            .WithInnerException<AmazonS3Exception>()
            .Which.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        (await sut.DownloadTextAsync("docs/a.txt")).Should().Be("first");
    }

    [DockerFact]
    public async Task Upload_NewPath_WithoutOverwrite_Succeeds()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket);
        var sut = provider.GetRequiredService<IFileStorageService>();

        await sut.UploadTextAsync("docs", "new.txt", "fresh", overwrite: false);

        (await sut.DownloadTextAsync("docs/new.txt")).Should().Be("fresh");
    }

    [DockerFact]
    public async Task List_WithPrefix_ReturnsOnlyMatchingEntriesWithMetadata()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket);
        var sut = provider.GetRequiredService<IFileStorageService>();
        await sut.UploadTextAsync("reports", "a.txt", "12345");
        await sut.UploadTextAsync("reports", "b.txt", "123");
        await sut.UploadTextAsync("reports-archive", "old.txt", "x");
        await sut.UploadTextAsync("other", "c.txt", "x");

        var entries = await sut.ListAsync("reports");

        entries.Select(e => e.FullPath).Should().BeEquivalentTo(["reports/a.txt", "reports/b.txt"]);
        var a = entries.Single(e => e.FileName == "a.txt");
        a.SizeInBytes.Should().Be(5);
        a.ModifiedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));
    }

    [DockerFact]
    public async Task List_EmptyDirectory_ReturnsEmptyList()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket);
        var sut = provider.GetRequiredService<IFileStorageService>();

        var entries = await sut.ListAsync("nothing-here");

        entries.Should().BeEmpty();
    }

    // The listing is recursive (ListObjectsV2 without a delimiter): the root lists nested objects too.
    [DockerFact]
    public async Task List_Root_ReturnsEveryEntryRecursively()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket);
        var sut = provider.GetRequiredService<IFileStorageService>();
        await sut.UploadTextAsync("", "root.txt", "x");
        await sut.UploadTextAsync("docs", "nested.txt", "x");

        var entries = await sut.ListAsync("");

        entries.Select(e => e.FullPath).Should().BeEquivalentTo(["root.txt", "docs/nested.txt"]);
    }

    [DockerFact]
    public async Task List_WithKeyPrefix_ReturnsPathsWithoutThePrefix()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket, keyPrefix: "tenantA");
        var sut = provider.GetRequiredService<IFileStorageService>();
        await sut.UploadTextAsync("docs", "a.txt", "a");

        var entries = await sut.ListAsync("docs");

        entries.Should().ContainSingle();
        entries[0].FullPath.Should().Be("docs/a.txt");
        entries[0].FileName.Should().Be("a.txt");
    }

    [DockerFact]
    public async Task List_WithKeyPrefix_FullPathRoundTripsToDownload()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket, keyPrefix: "tenantA");
        var sut = provider.GetRequiredService<IFileStorageService>();
        await sut.UploadTextAsync("docs", "a.txt", "tenant content");

        var listed = (await sut.ListAsync("")).Single();
        var text = await sut.DownloadTextAsync(listed.FullPath);

        text.Should().Be("tenant content");
    }

    [DockerFact]
    public async Task Download_Missing_ThrowsNotFoundFromTheSdk()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket);
        var sut = provider.GetRequiredService<IFileStorageService>();

        var act = () => sut.DownloadAsync("docs/missing.txt");

        (await act.Should().ThrowAsync<AmazonS3Exception>())
            .Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [DockerFact]
    public async Task Delete_Missing_DoesNotThrow()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket);
        var sut = provider.GetRequiredService<IFileStorageService>();

        var act = () => sut.DeleteAsync("docs/missing.txt");

        await act.Should().NotThrowAsync();
    }

    [DockerFact]
    public async Task KeyPrefix_IsAppliedToStoredKeys()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket, keyPrefix: "tenantA");
        var sut = provider.GetRequiredService<IFileStorageService>();

        await sut.UploadTextAsync("docs", "a.txt", "a");

        (await s3.ListKeysAsync(bucket)).Should().Equal("tenantA/docs/a.txt");
        (await sut.ExistsAsync("docs/a.txt")).Should().BeTrue();
        (await sut.DownloadTextAsync("docs/a.txt")).Should().Be("a");
    }

    // Proves rejection and that nothing is written. That no request is sent at all is shown by
    // AwsS3StorageServicePathTests (an unreachable endpoint yields ArgumentException, not a connection error).
    [DockerFact]
    public async Task TraversalPath_IsRejectedAndNothingIsWritten()
    {
        var bucket = await s3.CreateBucketAsync();
        await using var provider = s3.BuildProvider(bucket, keyPrefix: "tenantA");
        var sut = provider.GetRequiredService<IFileStorageService>();

        var upload = () => sut.UploadTextAsync("../other", "x.txt", "pwned");
        var download = () => sut.DownloadAsync("../other/x.txt");

        await upload.Should().ThrowAsync<ArgumentException>();
        await download.Should().ThrowAsync<ArgumentException>();
        (await s3.ListKeysAsync(bucket)).Should().BeEmpty();
    }
}
