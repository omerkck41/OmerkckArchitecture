using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Kck.Search.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kck.Search.Elasticsearch;

public sealed partial class ElasticsearchSearchService<T>(
    IOptionsMonitor<ElasticsearchOptions> options,
    ILogger<ElasticsearchSearchService<T>> logger) : ISearchService<T> where T : class
{
    private readonly ElasticsearchClient _client = CreateClient(options.CurrentValue);
    private readonly ElasticsearchOptions _options = options.CurrentValue;

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create index {IndexName}: {DebugInfo}")]
    private static partial void LogIndexCreateFailed(ILogger logger, string indexName, string debugInfo);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to index document {DocumentId} in {IndexName}")]
    private static partial void LogIndexDocumentFailed(ILogger logger, string documentId, string indexName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Bulk index had errors in {IndexName}: {ErrorCount} failures")]
    private static partial void LogBulkIndexErrors(ILogger logger, string indexName, int errorCount);

    private static ElasticsearchClient CreateClient(ElasticsearchOptions opts)
    {
        var settings = new ElasticsearchClientSettings(new Uri(opts.ConnectionString));

        if (!string.IsNullOrEmpty(opts.DefaultIndex))
            settings = settings.DefaultIndex(opts.DefaultIndex);

        if (!string.IsNullOrEmpty(opts.Username) && !string.IsNullOrEmpty(opts.Password))
            settings = settings.Authentication(new BasicAuthentication(opts.Username, opts.Password));

        return new ElasticsearchClient(settings);
    }

    public async Task CreateIndexAsync(string indexName, CancellationToken ct = default)
    {
        var response = await _client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .NumberOfShards(_options.NumberOfShards)
                .NumberOfReplicas(_options.NumberOfReplicas)), ct).ConfigureAwait(false);

        if (!response.Acknowledged)
        {
            LogIndexCreateFailed(logger, indexName, response.DebugInformation);
            throw new InvalidOperationException($"Failed to create index '{indexName}'.");
        }
    }

    public async Task<bool> IndexExistsAsync(string indexName, CancellationToken ct = default)
    {
        var response = await _client.Indices.ExistsAsync(indexName, ct).ConfigureAwait(false);
        return response.Exists;
    }

    public async Task DeleteIndexAsync(string indexName, CancellationToken ct = default)
    {
        await _client.Indices.DeleteAsync(indexName, ct).ConfigureAwait(false);
    }

    public async Task IndexDocumentAsync(string indexName, string documentId, T document, CancellationToken ct = default)
    {
        var response = await _client.IndexAsync(document, i => i
            .Index(indexName)
            .Id(documentId), ct).ConfigureAwait(false);

        if (!response.IsValidResponse)
            LogIndexDocumentFailed(logger, documentId, indexName);
    }

    public async Task BulkIndexAsync(string indexName, IEnumerable<T> documents, CancellationToken ct = default)
    {
        var response = await _client.BulkAsync(b => b
            .Index(indexName)
            .IndexMany(documents), ct).ConfigureAwait(false);

        if (response.Errors)
            LogBulkIndexErrors(logger, indexName, response.ItemsWithErrors.Count());
    }

    public async Task UpdateDocumentAsync(string indexName, string documentId, T document, CancellationToken ct = default)
    {
        await _client.UpdateAsync<T, T>(indexName, documentId,
            u => u.Doc(document), ct).ConfigureAwait(false);
    }

    public async Task DeleteDocumentAsync(string indexName, string documentId, CancellationToken ct = default)
    {
        await _client.DeleteAsync<T>(documentId, d => d.Index(indexName), ct).ConfigureAwait(false);
    }

    public async Task<T?> GetByIdAsync(string indexName, string documentId, CancellationToken ct = default)
    {
        var response = await _client.GetAsync<T>(indexName, documentId, ct).ConfigureAwait(false);
        return response.Found ? response.Source : null;
    }

    public async Task<SearchResult<T>> SearchAsync(Abstractions.SearchRequest request, CancellationToken ct = default)
    {
        var indexName = request.IndexName ?? _options.DefaultIndex
            ?? throw new InvalidOperationException("No index name specified and no default index configured.");

        var searchDescriptor = new SearchRequestDescriptor<T>(indexName)
            .From(request.From)
            .Size(request.Size)
            .Query(q => q.QueryString(qs => qs.Query(request.Query)));

        if (!string.IsNullOrEmpty(request.SortField))
        {
            searchDescriptor.Sort(s => s.Field(f => f
                .Field(request.SortField!)
                .Order(request.SortAscending ? SortOrder.Asc : SortOrder.Desc)));
        }

        var response = await _client.SearchAsync<T>(searchDescriptor, ct).ConfigureAwait(false);

        return new SearchResult<T>
        {
            Items = response.Documents.ToList(),
            TotalCount = response.Total
        };
    }
}
