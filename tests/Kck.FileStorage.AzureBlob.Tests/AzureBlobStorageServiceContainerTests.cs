using System.Diagnostics;
using AwesomeAssertions;
using Kck.Testing;
using Xunit;

namespace Kck.FileStorage.AzureBlob.Tests;

/// <summary>
/// The constructor used to call <c>CreateIfNotExists</c> synchronously: a blocking network call during DI resolution
/// that threw when the account was unreachable. The container is now created asynchronously on first use.
/// </summary>
public sealed class AzureBlobStorageServiceContainerTests
{
    // Azurite's well-known development account. Nothing listens on it here, so any request that reaches the network
    // fails with a connection error.
    private static AzureBlobStorageService CreateSut() => new(new StaticOptionsMonitor<AzureBlobOptions>(
        new AzureBlobOptions
        {
            ConnectionString = "UseDevelopmentStorage=true",
            ContainerName = "alpha",
            CreateContainerIfNotExists = true
        }));

    [Fact]
    public void Constructor_WithCreateContainerIfNotExists_DoesNotTouchTheNetwork()
    {
        var stopwatch = Stopwatch.StartNew();

        var act = () => CreateSut();

        act.Should().NotThrow();
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task ExistsAsync_WithEscapingPath_IsRejectedBeforeAnyNetworkCall()
    {
        var sut = CreateSut();

        var act = () => sut.ExistsAsync("../beta/x");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // The first-use container creation itself is exercised against Azurite in the F4 behaviour tests; a test against the
    // unreachable development endpoint costs the SDK's full retry schedule (~37 s) and proves nothing the two tests
    // above do not.
}
