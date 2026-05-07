using FluentAssertions;
using Kck.Search.Abstractions;
using Kck.Search.Elasticsearch;
using Kck.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.Elasticsearch;
using Xunit;

namespace Kck.Search.Elasticsearch.Tests;

/// <summary>
/// LS-FAZ-4.5: Real Elasticsearch container integration tests.
/// Verifies index lifecycle, document CRUD, and search against a real ES node.
/// </summary>
/// <remarks>
/// Requires Docker. CI: ubuntu-only (Category=Integration filter).
/// Uses ElasticsearchBuilder defaults (HTTPS, elastic:elastic).
/// DisableSslVerification=true because the container uses a self-signed cert.
/// </remarks>
[Trait("Category", "Integration")]
#pragma warning disable CA1001
public sealed class ElasticsearchSearchIntegrationTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly ElasticsearchContainer _es = new ElasticsearchBuilder()
        .Build();

    private const string IndexName = "test-docs";
    private ElasticsearchSearchService<EsTestDoc> _sut = default!;

    public async Task InitializeAsync()
    {
        await _es.StartAsync();

        var options = new StaticOptionsMonitor<ElasticsearchOptions>(new ElasticsearchOptions
        {
            ConnectionString = _es.GetConnectionString(),
            Username = ElasticsearchBuilder.DefaultUsername,
            Password = ElasticsearchBuilder.DefaultPassword,
            DisableSslVerification = true,
            DefaultIndex = IndexName,
            NumberOfShards = 1,
            NumberOfReplicas = 0
        });

        _sut = new ElasticsearchSearchService<EsTestDoc>(
            options,
            NullLogger<ElasticsearchSearchService<EsTestDoc>>.Instance);

        await _sut.CreateIndexAsync(IndexName);
    }

    public async Task DisposeAsync()
    {
        await _es.DisposeAsync();
    }

    [Fact]
    public async Task IndexExistsAsync_AfterCreate_ReturnsTrue()
    {
        var exists = await _sut.IndexExistsAsync(IndexName);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task IndexDocumentAsync_GetByIdAsync_RoundTrip()
    {
        var doc = new EsTestDoc { Id = Guid.NewGuid().ToString(), Name = "Integration Test" };

        await _sut.IndexDocumentAsync(IndexName, doc.Id, doc);
        await Task.Delay(1000);

        var fetched = await _sut.GetByIdAsync(IndexName, doc.Id);

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

        await _sut.BulkIndexAsync(IndexName, docs);
        await Task.Delay(1500);

        var result = await _sut.SearchAsync(new SearchRequest { Query = "Bulk*", Size = 10 });

        result.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        result.Items.Should().Contain(d => d.Name.StartsWith("Bulk", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DeleteDocumentAsync_GetByIdAsync_ReturnsNull()
    {
        var doc = new EsTestDoc { Id = Guid.NewGuid().ToString(), Name = "ToDelete" };
        await _sut.IndexDocumentAsync(IndexName, doc.Id, doc);
        await Task.Delay(1000);

        await _sut.DeleteDocumentAsync(IndexName, doc.Id);
        await Task.Delay(500);

        var result = await _sut.GetByIdAsync(IndexName, doc.Id);
        result.Should().BeNull();
    }
}

internal sealed class EsTestDoc
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}
