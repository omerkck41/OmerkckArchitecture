using Azure.Storage.Blobs;
using Kck.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Azurite;
using Xunit;

namespace Kck.FileStorage.AzureBlob.Tests;

/// <summary>
/// One Azurite container shared by every test of a class, plus a raw <see cref="BlobServiceClient"/> used to arrange
/// and inspect containers and blobs independently of the service under test. Azurite uses its well-known development
/// account; the connection string comes from <see cref="AzuriteContainer.GetConnectionString"/>.
/// </summary>
#pragma warning disable CA1001 // disposed through IAsyncLifetime.DisposeAsync
public sealed class AzuriteContainerFixture : IAsyncLifetime
#pragma warning restore CA1001
{
    // Pinned explicitly; the module's default image is the older 3.28.0.
    internal const string Image = "mcr.microsoft.com/azure-storage/azurite:3.35.0";

    // Azure.Storage.Blobs sends a newer service version (2026-10-06) than Azurite 3.35.0 knows and Azurite rejects it
    // with 400 InvalidHeaderValue; --skipApiVersionCheck lets it serve the request with its own implementation.
    private readonly AzuriteContainer _container = new AzuriteBuilder(Image)
        .WithCommand("--skipApiVersionCheck")
        .Build();
    private BlobServiceClient? _raw;

    /// <summary>The raw client, for arranging and inspecting blobs without the service under test.</summary>
    public BlobServiceClient Raw => _raw ?? throw new InvalidOperationException("The fixture is not initialized.");

    public async Task InitializeAsync()
    {
        // xUnit v2 creates class fixtures even when every test of the class is skipped; every user of this
        // fixture is a [DockerFact], so do not spend time on a daemon the attribute already found missing.
        if (DockerProbe.CurrentSkipReason() is not null)
            return;

        await _container.StartAsync();
        _raw = new BlobServiceClient(_container.GetConnectionString());
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    /// <summary>A container name no other test uses; the container itself is not created.</summary>
    public static string UniqueContainerName() => "kck-" + Guid.NewGuid().ToString("N");

    /// <summary>
    /// Builds the service under test through the real registration (<c>AddKckFileStorageAzureBlob</c>, options
    /// validation included).
    /// </summary>
    public ServiceProvider BuildProvider(string containerName, bool createContainerIfNotExists = true)
    {
        var services = new ServiceCollection();
        services.AddKckFileStorageAzureBlob(o =>
        {
            o.ConnectionString = _container.GetConnectionString();
            o.ContainerName = containerName;
            o.CreateContainerIfNotExists = createContainerIfNotExists;
        });
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
    }

    /// <summary>Whether <paramref name="containerName"/> exists, read with the raw client.</summary>
    public async Task<bool> ContainerExistsAsync(string containerName) =>
        (await Raw.GetBlobContainerClient(containerName).ExistsAsync()).Value;

    /// <summary>Every blob name in <paramref name="containerName"/>, read with the raw client.</summary>
    public async Task<IReadOnlyList<string>> ListBlobNamesAsync(string containerName)
    {
        var names = new List<string>();
        await foreach (var item in Raw.GetBlobContainerClient(containerName).GetBlobsAsync())
            names.Add(item.Name);
        return names;
    }
}
