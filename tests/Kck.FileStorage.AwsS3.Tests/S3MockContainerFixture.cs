using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Kck.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kck.FileStorage.AwsS3.Tests;

/// <summary>
/// One Adobe S3Mock container shared by every test of a class, plus a raw S3 client (built from the same
/// <see cref="AwsS3StorageService.CreateClientConfig"/> as the provider) used to create buckets and inspect object
/// keys independently of the service under test. The official MinIO image is no longer pullable, so the S3 API is
/// served by <c>adobe/s3mock</c> through the generic <see cref="ContainerBuilder"/>
/// (decisions/2026-11-02-testcontainers-minio-azurite.md).
/// </summary>
#pragma warning disable CA1001 // disposed through IAsyncLifetime.DisposeAsync
public sealed class S3MockContainerFixture : IAsyncLifetime
#pragma warning restore CA1001
{
    internal const string Image = "adobe/s3mock:5.2.3";
    private const ushort S3Port = 9090;

    // Signs requests only; S3Mock accepts any region.
    private const string Region = "us-east-1";

    // S3Mock does not authenticate; the SDK still needs a key pair to sign requests. Placeholders, not credentials.
    private const string AccessKey = "kck-test-access-key";
    private const string SecretKey = "kck-test-secret-key";

    private readonly IContainer _container;
    private AmazonS3Client? _raw;

    public S3MockContainerFixture()
    {
        _container = new ContainerBuilder(Image)
            .WithPortBinding(S3Port, assignRandomHostPort: true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Started S3MockApplication"))
            .Build();
    }

    private string ServiceUrl => $"http://{_container.Hostname}:{_container.GetMappedPublicPort(S3Port)}";

    /// <summary>The raw client, for arranging and inspecting objects without the service under test.</summary>
    public IAmazonS3 Raw => _raw ?? throw new InvalidOperationException("The fixture is not initialized.");

    public async Task InitializeAsync()
    {
        // xUnit v2 creates class fixtures even when every test of the class is skipped; every user of this
        // fixture is a [DockerFact], so do not spend time on a daemon the attribute already found missing.
        if (DockerProbe.CurrentSkipReason() is not null)
            return;

        await _container.StartAsync();

        var config = AwsS3StorageService.CreateClientConfig(CreateOptions("unused", keyPrefix: null));
        _raw = new AmazonS3Client(new BasicAWSCredentials(AccessKey, SecretKey), config);
    }

    public async Task DisposeAsync()
    {
        _raw?.Dispose();
        await _container.DisposeAsync();
    }

    /// <summary>Creates an empty bucket with a unique name, so tests never share objects.</summary>
    public async Task<string> CreateBucketAsync()
    {
        var bucket = "kck-" + Guid.NewGuid().ToString("N");
        await Raw.PutBucketAsync(new PutBucketRequest { BucketName = bucket });
        return bucket;
    }

    /// <summary>
    /// Builds the service under test through the real registration (<c>AddKckFileStorageAwsS3</c>, options
    /// validation included). Dispose the provider to dispose the service.
    /// </summary>
    public ServiceProvider BuildProvider(string bucket, string? keyPrefix = null)
    {
        var expected = CreateOptions(bucket, keyPrefix);
        var services = new ServiceCollection();
        services.AddKckFileStorageAwsS3(o =>
        {
            o.Region = expected.Region;
            o.BucketName = expected.BucketName;
            o.KeyPrefix = expected.KeyPrefix;
            o.ServiceUrl = expected.ServiceUrl;
            o.ForcePathStyle = expected.ForcePathStyle;
            o.AccessKey = expected.AccessKey;
            o.SecretKey = expected.SecretKey;
        });
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
    }

    /// <summary>Every object key in <paramref name="bucket"/>, read with the raw client.</summary>
    public async Task<IReadOnlyList<string>> ListKeysAsync(string bucket)
    {
        var keys = new List<string>();
        var request = new ListObjectsV2Request { BucketName = bucket };
        ListObjectsV2Response response;
        do
        {
            response = await Raw.ListObjectsV2Async(request);
            keys.AddRange((response.S3Objects ?? []).Select(o => o.Key));
            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated == true);

        return keys;
    }

    private AwsS3Options CreateOptions(string bucket, string? keyPrefix) => new()
    {
        Region = Region,
        BucketName = bucket,
        KeyPrefix = keyPrefix,
        ServiceUrl = ServiceUrl,
        ForcePathStyle = true,
        AccessKey = AccessKey,
        SecretKey = SecretKey
    };
}
