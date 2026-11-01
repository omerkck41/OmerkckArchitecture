using Elastic.Clients.Elasticsearch;
using AwesomeAssertions;
using Kck.Search.Elasticsearch;
using Kck.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.Elasticsearch;
using Xunit;

namespace Kck.Search.Elasticsearch.Tests;

/// <summary>
/// Starts one Elasticsearch container for every test of <see cref="ElasticsearchIntegrationTests"/>.
/// </summary>
public sealed class ElasticsearchContainerFixture : IAsyncLifetime
{
    private readonly ElasticsearchContainer _container;

    public ElasticsearchContainerFixture()
    {
        // Elastic.Clients.Elasticsearch 9.x sends Accept: compatible-with=9 which ES 8.x rejects.
        // Container must match the client major version (9.x).
        // A per-run random credential avoids relying on Testcontainers default credential detection.
        ElasticCredential = Guid.NewGuid().ToString("N");
        _container = new ElasticsearchBuilder("docker.elastic.co/elasticsearch/elasticsearch:9.0.1")
            .WithPassword(ElasticCredential)
            .Build();
    }

    public string ElasticCredential { get; }

    public string BaseUrl { get; private set; } = "";

    public async Task InitializeAsync()
    {
        // xUnit v2 creates class fixtures even when every test of the class is skipped; every test here is a
        // [DockerFact], so do not try to start a container the attribute already found unreachable.
        if (DockerProbe.CurrentSkipReason() is not null)
            return;

        await _container.StartAsync();

        var rawUri = new Uri(_container.GetConnectionString());
        BaseUrl = $"{rawUri.Scheme}://{rawUri.Host}:{rawUri.Port}";
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}

/// <summary>
/// Faz-B1: Real Elasticsearch container integration tests.
/// Verifies index lifecycle, document CRUD, bulk indexing, search and the error contract of the write operations.
/// </summary>
/// <remarks>
/// Requires Docker. CI: ubuntu-only (Category=Integration filter). One container serves the whole class, so every
/// test uses its own index.
/// </remarks>
[Trait("Category", "Integration")]
public sealed class ElasticsearchIntegrationTests : IClassFixture<ElasticsearchContainerFixture>, IDisposable
{
    private readonly ElasticsearchContainerFixture _fixture;
    private readonly ElasticsearchSearchService<TestDoc> _sut;
    private readonly ElasticsearchClient _directClient;
    private readonly ElasticsearchClientSettings _clientSettings;

    public ElasticsearchIntegrationTests(ElasticsearchContainerFixture fixture)
    {
        _fixture = fixture;
        _sut = CreateService(useSimpleQueryString: false);

        // Assigned before the fluent configuration so the instance is visibly owned (and disposed) by this class.
        _clientSettings = new ElasticsearchClientSettings(new Uri(fixture.BaseUrl));
        _clientSettings.Authentication(new Elastic.Transport.BasicAuthentication("elastic", fixture.ElasticCredential));
        _clientSettings.ServerCertificateValidationCallback((_, _, _, _) => true);
        _directClient = new ElasticsearchClient(_clientSettings);
    }

    public void Dispose()
    {
        _sut.Dispose();
        ((IDisposable)_clientSettings).Dispose();
    }

    [DockerFact]
    public async Task CreateIndexAsync_WhenIndexDoesNotExist_CreatesSuccessfully()
    {
        const string index = "kck-it-create-index";

        await _sut.CreateIndexAsync(index);

        (await _sut.IndexExistsAsync(index)).Should().BeTrue();
    }

    [DockerFact]
    public async Task IndexDocumentAsync_GetByIdAsync_RoundTrip()
    {
        const string index = "kck-it-round-trip";
        await EnsureIndexAsync(index);
        var doc = new TestDoc { Id = Guid.NewGuid().ToString(), Name = "integration-test", Score = 42 };

        await _sut.IndexDocumentAsync(index, doc.Id, doc);

        var retrieved = await _sut.GetByIdAsync(index, doc.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be(doc.Name);
        retrieved.Score.Should().Be(doc.Score);
    }

    [DockerFact]
    public async Task DeleteDocumentAsync_AfterIndex_DocumentNoLongerExists()
    {
        const string index = "kck-it-delete-document";
        await EnsureIndexAsync(index);
        var doc = new TestDoc { Id = Guid.NewGuid().ToString(), Name = "to-delete", Score = 1 };
        await _sut.IndexDocumentAsync(index, doc.Id, doc);

        await _sut.DeleteDocumentAsync(index, doc.Id);

        var retrieved = await _sut.GetByIdAsync(index, doc.Id);
        retrieved.Should().BeNull();
    }

    [DockerFact]
    public async Task BulkIndexAsync_IndexesAllDocuments()
    {
        const string index = "kck-it-bulk-index";
        await EnsureIndexAsync(index);
        var docs = Enumerable.Range(1, 5)
            .Select(i => new TestDoc { Id = $"bulk-{i}", Name = $"item-{i}", Score = i })
            .ToList();

        await _sut.BulkIndexAsync(index, docs);
        await _directClient.Indices.RefreshAsync(index);

        foreach (var doc in docs)
        {
            var retrieved = await _sut.GetByIdAsync(index, doc.Id);
            retrieved.Should().NotBeNull($"doc {doc.Id} should exist after bulk index");
        }
    }

    [DockerFact]
    public async Task SearchAsync_AfterIndexAndRefresh_ReturnsMatchingDocuments()
    {
        const string index = "kck-it-search";
        await EnsureIndexAsync(index);
        var uniqueToken = Guid.NewGuid().ToString("N");
        var doc = new TestDoc { Id = uniqueToken, Name = $"searchable-{uniqueToken}", Score = 99 };
        await _sut.IndexDocumentAsync(index, doc.Id, doc);
        await _directClient.Indices.RefreshAsync(index);

        var result = await _sut.SearchAsync(new Kck.Search.Abstractions.SearchRequest
        {
            Query = uniqueToken,
            IndexName = index,
            Size = 10
        });

        result.Items.Should().NotBeEmpty("indexed document should be findable by its unique token");
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [DockerFact]
    public async Task IndexExistsAsync_WhenIndexDoesNotExist_ReturnsFalse()
    {
        (await _sut.IndexExistsAsync("nonexistent-index-kck-test")).Should().BeFalse();
    }

    [DockerFact]
    public async Task IndexDocumentAsync_WithInvalidIndexName_Throws()
    {
        var doc = new TestDoc { Id = "invalid-1", Name = "rejected", Score = 1 };

        var act = () => _sut.IndexDocumentAsync("Invalid Name", doc.Id, doc);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("index").And.Contain("Invalid Name")
            .And.Contain("invalid_index_name_exception");
    }

    [DockerFact]
    public async Task BulkIndexAsync_WithInvalidIndexName_Throws()
    {
        var docs = new[] { new TestDoc { Id = "invalid-bulk-1", Name = "rejected", Score = 1 } };

        var act = () => _sut.BulkIndexAsync("Invalid Name", docs);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("bulk index").And.Contain("Invalid Name");
    }

    [DockerFact]
    public async Task UpdateDocumentAsync_WhenDocumentMissing_Throws()
    {
        const string index = "kck-it-update-missing";
        await EnsureIndexAsync(index);
        var doc = new TestDoc { Id = "missing-doc", Name = "never-indexed", Score = 1 };

        var act = () => _sut.UpdateDocumentAsync(index, doc.Id, doc);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("update").And.Contain(index).And.Contain("missing-doc")
            .And.Contain("document_missing_exception");
    }

    [DockerFact]
    public async Task DeleteDocumentAsync_WhenDocumentMissing_DoesNotThrow()
    {
        const string index = "kck-it-delete-missing-document";
        await EnsureIndexAsync(index);

        var act = () => _sut.DeleteDocumentAsync(index, "missing-doc");

        await act.Should().NotThrowAsync();
    }

    [DockerFact]
    public async Task DeleteIndexAsync_WhenIndexMissing_DoesNotThrow()
    {
        var act = () => _sut.DeleteIndexAsync("kck-it-never-created");

        await act.Should().NotThrowAsync();
    }

    [DockerFact]
    public async Task SearchAsync_WithSimpleQueryString_DoesNotThrowOnLuceneSyntaxError()
    {
        const string index = "kck-it-simple-syntax-error";
        await EnsureIndexAsync(index);
        var uniqueToken = Guid.NewGuid().ToString("N");
        var doc = new TestDoc { Id = uniqueToken, Name = $"tolerant-{uniqueToken}", Score = 7 };
        await _sut.IndexDocumentAsync(index, doc.Id, doc);
        await _directClient.Indices.RefreshAsync(index);
        using var simple = CreateService(useSimpleQueryString: true);

        // "name:(" is a Lucene syntax error: with query_string Elasticsearch answers 400 and nothing is found.
        var result = await simple.SearchAsync(new Kck.Search.Abstractions.SearchRequest
        {
            Query = $"name:( {uniqueToken}",
            IndexName = index,
            Size = 10
        });

        result.Items.Should().NotBeEmpty("simple_query_string ignores the broken operator and still matches the term");
    }

    [DockerFact]
    public async Task SearchAsync_WithSimpleQueryString_FindsDocumentByTerm()
    {
        const string index = "kck-it-simple-term";
        await EnsureIndexAsync(index);
        var uniqueToken = Guid.NewGuid().ToString("N");
        var doc = new TestDoc { Id = uniqueToken, Name = $"plain-{uniqueToken}", Score = 3 };
        await _sut.IndexDocumentAsync(index, doc.Id, doc);
        await _directClient.Indices.RefreshAsync(index);
        using var simple = CreateService(useSimpleQueryString: true);

        var result = await simple.SearchAsync(new Kck.Search.Abstractions.SearchRequest
        {
            Query = uniqueToken,
            IndexName = index,
            Size = 10
        });

        result.Items.Should().ContainSingle().Which.Id.Should().Be(uniqueToken);
        result.TotalCount.Should().Be(1);
    }

    [DockerFact]
    public async Task BulkIndexAsync_WithNoDocuments_DoesNothing()
    {
        const string index = "kck-it-bulk-empty";

        var act = () => _sut.BulkIndexAsync(index, Enumerable.Empty<TestDoc>());

        await act.Should().NotThrowAsync("an empty batch sends no request");
        (await _sut.IndexExistsAsync(index)).Should().BeFalse("nothing was sent, so no index was auto-created");
    }

    [DockerFact]
    public async Task SearchAsync_WithUnknownIndex_Throws()
    {
        var act = () => _sut.SearchAsync(new Kck.Search.Abstractions.SearchRequest
        {
            Query = "anything",
            IndexName = "kck-it-search-never-created",
            Size = 10
        });

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("search").And.Contain("kck-it-search-never-created")
            .And.Contain("404").And.Contain("index_not_found_exception");
    }

    [DockerFact]
    public async Task SearchAsync_DefaultMode_WithLuceneSyntaxError_Throws()
    {
        const string index = "kck-it-search-syntax-error";
        await EnsureIndexAsync(index);

        // "name:(" is a Lucene syntax error: query_string rejects it with 400 instead of returning no hits.
        var act = () => _sut.SearchAsync(new Kck.Search.Abstractions.SearchRequest
        {
            Query = "name:( broken",
            IndexName = index,
            Size = 10
        });

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("search").And.Contain(index).And.Contain("400");
    }

    [DockerFact]
    public async Task SearchAsync_WithSimpleQueryString_DoesNotPrefixMatch()
    {
        const string index = "kck-it-simple-prefix";
        await EnsureIndexAsync(index);
        var doc = new TestDoc { Id = "prefix-doc", Name = "alphabet", Score = 5 };
        await _sut.IndexDocumentAsync(index, doc.Id, doc);
        await _directClient.Indices.RefreshAsync(index);
        using var simple = CreateService(useSimpleQueryString: true);
        var request = new Kck.Search.Abstractions.SearchRequest { Query = "alpha*", IndexName = index, Size = 10 };

        var simpleResult = await simple.SearchAsync(request);
        var defaultResult = await _sut.SearchAsync(request);

        simpleResult.Items.Should().BeEmpty("simple mode does not enable the PREFIX operator (no wildcard expansion)");
        defaultResult.Items.Should().ContainSingle("query_string expands the prefix")
            .Which.Id.Should().Be("prefix-doc");
    }

    private ElasticsearchSearchService<TestDoc> CreateService(bool useSimpleQueryString)
    {
        var opts = new ElasticsearchOptions
        {
            ConnectionString = _fixture.BaseUrl,
            Username = "elastic",
            Password = _fixture.ElasticCredential,
            NumberOfShards = 1,
            NumberOfReplicas = 0,
            DisableSslVerification = true,
            UseSimpleQueryString = useSimpleQueryString
        };

        return new ElasticsearchSearchService<TestDoc>(
            new StaticOptionsMonitor<ElasticsearchOptions>(opts),
            NullLogger<ElasticsearchSearchService<TestDoc>>.Instance);
    }

    private async Task EnsureIndexAsync(string index)
    {
        if (!await _sut.IndexExistsAsync(index))
            await _sut.CreateIndexAsync(index);
    }
}

internal sealed class TestDoc
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Score { get; set; }
}
