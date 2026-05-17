using Elastic.Clients.Elasticsearch;
using FluentAssertions;
using Kck.Search.Elasticsearch;
using Kck.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.Elasticsearch;
using Xunit;

namespace Kck.Search.Elasticsearch.Tests;

/// <summary>
/// Faz-B1: Real Elasticsearch container integration tests.
/// Verifies index lifecycle, document CRUD, bulk indexing, and search.
/// </summary>
/// <remarks>
/// Requires Docker. CI: ubuntu-only (Category=Integration filter).
/// </remarks>
[Trait("Category", "Integration")]
#pragma warning disable CA1001
public sealed class ElasticsearchIntegrationTests : IAsyncLifetime
#pragma warning restore CA1001
{
    // Elastic.Clients.Elasticsearch 9.x sends Accept: compatible-with=9 which ES 8.x rejects.
    // Container must match the client major version (9.x).
    // Explicit password avoids relying on Testcontainers default credential detection.
    private const string ElasticPassword = "elastic_test";

    private readonly ElasticsearchContainer _container =
        new ElasticsearchBuilder("docker.elastic.co/elasticsearch/elasticsearch:9.0.1")
            .WithPassword(ElasticPassword)
            .Build();

    private ElasticsearchSearchService<TestDoc> _sut = default!;
    private ElasticsearchClient _directClient = default!;
    private const string TestIndex = "kck-integration-test";

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var rawUri = new Uri(_container.GetConnectionString());
        var baseUrl = $"{rawUri.Scheme}://{rawUri.Host}:{rawUri.Port}";

        var opts = new ElasticsearchOptions
        {
            ConnectionString = baseUrl,
            Username = "elastic",
            Password = ElasticPassword,
            DefaultIndex = TestIndex,
            NumberOfShards = 1,
            NumberOfReplicas = 0,
            DisableSslVerification = true
        };

        _sut = new ElasticsearchSearchService<TestDoc>(
            new StaticOptionsMonitor<ElasticsearchOptions>(opts),
            NullLogger<ElasticsearchSearchService<TestDoc>>.Instance);

        using var settings = new ElasticsearchClientSettings(new Uri(baseUrl))
            .Authentication(new Elastic.Transport.BasicAuthentication("elastic", ElasticPassword))
            .ServerCertificateValidationCallback((_, _, _, _) => true);
        _directClient = new ElasticsearchClient(settings);
    }

    public async Task DisposeAsync()
    {
        if (await _sut.IndexExistsAsync(TestIndex))
            await _sut.DeleteIndexAsync(TestIndex);
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task CreateIndexAsync_WhenIndexDoesNotExist_CreatesSuccessfully()
    {
        await _sut.CreateIndexAsync(TestIndex);

        (await _sut.IndexExistsAsync(TestIndex)).Should().BeTrue();
    }

    [Fact]
    public async Task IndexDocumentAsync_GetByIdAsync_RoundTrip()
    {
        await EnsureIndexAsync();
        var doc = new TestDoc { Id = Guid.NewGuid().ToString(), Name = "integration-test", Score = 42 };

        await _sut.IndexDocumentAsync(TestIndex, doc.Id, doc);

        var retrieved = await _sut.GetByIdAsync(TestIndex, doc.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be(doc.Name);
        retrieved.Score.Should().Be(doc.Score);
    }

    [Fact]
    public async Task DeleteDocumentAsync_AfterIndex_DocumentNoLongerExists()
    {
        await EnsureIndexAsync();
        var doc = new TestDoc { Id = Guid.NewGuid().ToString(), Name = "to-delete", Score = 1 };
        await _sut.IndexDocumentAsync(TestIndex, doc.Id, doc);

        await _sut.DeleteDocumentAsync(TestIndex, doc.Id);

        var retrieved = await _sut.GetByIdAsync(TestIndex, doc.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task BulkIndexAsync_IndexesAllDocuments()
    {
        await EnsureIndexAsync();
        var docs = Enumerable.Range(1, 5)
            .Select(i => new TestDoc { Id = $"bulk-{i}", Name = $"item-{i}", Score = i })
            .ToList();

        await _sut.BulkIndexAsync(TestIndex, docs);
        await _directClient.Indices.RefreshAsync(TestIndex);

        foreach (var doc in docs)
        {
            var retrieved = await _sut.GetByIdAsync(TestIndex, doc.Id);
            retrieved.Should().NotBeNull($"doc {doc.Id} should exist after bulk index");
        }
    }

    [Fact]
    public async Task SearchAsync_AfterIndexAndRefresh_ReturnsMatchingDocuments()
    {
        await EnsureIndexAsync();
        var uniqueToken = Guid.NewGuid().ToString("N");
        var doc = new TestDoc { Id = uniqueToken, Name = $"searchable-{uniqueToken}", Score = 99 };
        await _sut.IndexDocumentAsync(TestIndex, doc.Id, doc);
        await _directClient.Indices.RefreshAsync(TestIndex);

        var result = await _sut.SearchAsync(new Kck.Search.Abstractions.SearchRequest
        {
            Query = uniqueToken,
            IndexName = TestIndex,
            Size = 10
        });

        result.Items.Should().NotBeEmpty("indexed document should be findable by its unique token");
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task IndexExistsAsync_WhenIndexDoesNotExist_ReturnsFalse()
    {
        (await _sut.IndexExistsAsync("nonexistent-index-kck-test")).Should().BeFalse();
    }

    private async Task EnsureIndexAsync()
    {
        if (!await _sut.IndexExistsAsync(TestIndex))
            await _sut.CreateIndexAsync(TestIndex);
    }
}

internal sealed class TestDoc
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Score { get; set; }
}
