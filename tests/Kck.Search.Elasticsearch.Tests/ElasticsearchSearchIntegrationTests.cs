using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Kck.Search.Abstractions;
using Kck.Search.Elasticsearch;
using Kck.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kck.Search.Elasticsearch.Tests;

/// <summary>
/// Shared Elasticsearch container for all integration tests in this assembly.
/// xUnit Collection Fixture: container is started once, shared across all test methods.
/// </summary>
public sealed class ElasticsearchFixture : IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder()
        .WithImage("docker.elastic.co/elasticsearch/elasticsearch:8.13.4")
        .WithEnvironment("xpack.security.enabled", "false")
        .WithEnvironment("discovery.type", "single-node")
        .WithEnvironment("ES_JAVA_OPTS", "-Xms512m -Xmx512m")
        .WithPortBinding(9200, true)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(r => r
                .ForPort(9200)
                .ForPath("/_cluster/health")))
        .Build();

    internal ElasticsearchSearchService<EsTestDoc> Service { get; private set; } = default!;
    public const string IndexName = "test-docs";

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var connStr = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(9200)}";
        var options = new StaticOptionsMonitor<ElasticsearchOptions>(new ElasticsearchOptions
        {
            ConnectionString = connStr,
            DefaultIndex = IndexName,
            NumberOfShards = 1,
            NumberOfReplicas = 0
        });

        Service = new ElasticsearchSearchService<EsTestDoc>(
            options,
            NullLogger<ElasticsearchSearchService<EsTestDoc>>.Instance);

        await Service.CreateIndexAsync(IndexName);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition("Elasticsearch")]
public class ElasticsearchShared : ICollectionFixture<ElasticsearchFixture> { }

/// <summary>
/// LS-FAZ-4.5: Real Elasticsearch container integration tests.
/// Verifies index lifecycle, document CRUD, and search against a real ES node.
/// </summary>
/// <remarks>
/// Requires Docker. CI: ubuntu-only (Category=Integration filter).
/// Single container instance shared via xUnit Collection Fixture.
/// Security disabled — plain HTTP, no cert management.
/// </remarks>
[Collection("Elasticsearch")]
[Trait("Category", "Integration")]
public sealed class ElasticsearchSearchIntegrationTests
{
    private readonly ElasticsearchSearchService<EsTestDoc> _sut;

    public ElasticsearchSearchIntegrationTests(ElasticsearchFixture fixture)
    {
        _sut = fixture.Service;
    }

    [Fact]
    public async Task IndexExistsAsync_AfterCreate_ReturnsTrue()
    {
        var exists = await _sut.IndexExistsAsync(ElasticsearchFixture.IndexName);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task IndexDocumentAsync_GetByIdAsync_RoundTrip()
    {
        var doc = new EsTestDoc { Id = Guid.NewGuid().ToString(), Name = "Integration Test" };

        await _sut.IndexDocumentAsync(ElasticsearchFixture.IndexName, doc.Id, doc);
        await Task.Delay(1000);

        var fetched = await _sut.GetByIdAsync(ElasticsearchFixture.IndexName, doc.Id);

        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Integration Test");
    }

    [Fact]
    public async Task BulkIndexAsync_SearchAsync_FindsDocuments()
    {
        var docs = new[]
        {
            new EsTestDoc { Id = Guid.NewGuid().ToString(), Name = "BulkAlpha" },
            new EsTestDoc { Id = Guid.NewGuid().ToString(), Name = "BulkBeta" }
        };

        await _sut.BulkIndexAsync(ElasticsearchFixture.IndexName, docs);
        await Task.Delay(1500);

        var result = await _sut.SearchAsync(new SearchRequest { Query = "Bulk*", Size = 10 });

        result.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        result.Items.Should().Contain(d => d.Name.StartsWith("Bulk", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DeleteDocumentAsync_GetByIdAsync_ReturnsNull()
    {
        var doc = new EsTestDoc { Id = Guid.NewGuid().ToString(), Name = "ToDelete" };
        await _sut.IndexDocumentAsync(ElasticsearchFixture.IndexName, doc.Id, doc);
        await Task.Delay(1000);

        await _sut.DeleteDocumentAsync(ElasticsearchFixture.IndexName, doc.Id);
        await Task.Delay(500);

        var result = await _sut.GetByIdAsync(ElasticsearchFixture.IndexName, doc.Id);
        result.Should().BeNull();
    }
}

internal sealed class EsTestDoc
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}
