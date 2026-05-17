using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Kck.Search.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kck.Search.Elasticsearch;

/// <summary>
/// Elasticsearch-backed implementation of <see cref="ISearchService{T}"/> that supports index management, document CRUD, bulk indexing, and full-text search for documents of type <typeparamref name="T"/>.
/// </summary>
public sealed partial class ElasticsearchSearchService<T> : ISearchService<T> where T : class
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly ILogger<ElasticsearchSearchService<T>> _logger;

    /// <summary>
    /// Initializes the service, constructing an <see cref="ElasticsearchClient"/> from the supplied options including optional basic authentication and SSL settings.
    /// </summary>
    public ElasticsearchSearchService(
        IOptionsMonitor<ElasticsearchOptions> options,
        ILogger<ElasticsearchSearchService<T>> logger)
    {
        _options = options.CurrentValue;
        _logger = logger;
        _client = new ElasticsearchClient(CreateSettings(_options));
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create index {IndexName}: {DebugInfo}")]
    private static partial void LogIndexCreateFailed(ILogger logger, string indexName, string debugInfo);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to index document {DocumentId} in {IndexName}")]
    private static partial void LogIndexDocumentFailed(ILogger logger, string documentId, string indexName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Bulk index had errors in {IndexName}: {ErrorCount} failures")]
    private static partial void LogBulkIndexErrors(ILogger logger, string indexName, int errorCount);

    private static ElasticsearchClientSettings CreateSettings(ElasticsearchOptions opts)
    {
        var settings = new ElasticsearchClientSettings(new Uri(opts.ConnectionString));

        if (!string.IsNullOrEmpty(opts.DefaultIndex))
            settings = settings.DefaultIndex(opts.DefaultIndex);

        if (!string.IsNullOrEmpty(opts.Username) && !string.IsNullOrEmpty(opts.Password))
            settings = settings.Authentication(new BasicAuthentication(opts.Username, opts.Password));

        if (opts.DisableSslVerification)
            settings = settings.ServerCertificateValidationCallback((_, _, _, _) => true);

        return settings;
    }

    /// <summary>Creates a new Elasticsearch index with the shard and replica counts from options, throwing if the cluster does not acknowledge the creation.</summary>
    public async Task CreateIndexAsync(string indexName, CancellationToken ct = default)
    {
        var response = await _client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .NumberOfShards(_options.NumberOfShards)
                .NumberOfReplicas(_options.NumberOfReplicas)), ct).ConfigureAwait(false);

        if (!response.Acknowledged)
        {
            LogIndexCreateFailed(_logger, indexName, response.DebugInformation);
            throw new InvalidOperationException(
                $"Failed to create index '{indexName}'. ES response: {response.DebugInformation}");
        }
    }

    /// <summary>Returns <see langword="true"/> if an index with the given name exists in the Elasticsearch cluster.</summary>
    public async Task<bool> IndexExistsAsync(string indexName, CancellationToken ct = default)
    {
        var response = await _client.Indices.ExistsAsync(indexName, ct).ConfigureAwait(false);
        return response.Exists;
    }

    /// <summary>Deletes the specified index from Elasticsearch.</summary>
    public async Task DeleteIndexAsync(string indexName, CancellationToken ct = default)
    {
        await _client.Indices.DeleteAsync(indexName, ct).ConfigureAwait(false);
    }

    /// <summary>Indexes or replaces a single document with the given <paramref name="documentId"/> in the specified index.</summary>
    public async Task IndexDocumentAsync(string indexName, string documentId, T document, CancellationToken ct = default)
    {
        var response = await _client.IndexAsync(document, i => i
            .Index(indexName)
            .Id(documentId), ct).ConfigureAwait(false);

        if (!response.IsValidResponse)
            LogIndexDocumentFailed(_logger, documentId, indexName);
    }

    /// <summary>Sends all <paramref name="documents"/> to Elasticsearch in a single bulk request, logging a warning if any item fails.</summary>
    public async Task BulkIndexAsync(string indexName, IEnumerable<T> documents, CancellationToken ct = default)
    {
        var response = await _client.BulkAsync(b => b
            .Index(indexName)
            .IndexMany(documents), ct).ConfigureAwait(false);

        if (response.Errors)
            LogBulkIndexErrors(_logger, indexName, response.ItemsWithErrors.Count());
    }

    /// <summary>Partially updates the document identified by <paramref name="documentId"/> with the fields present in <paramref name="document"/>.</summary>
    public async Task UpdateDocumentAsync(string indexName, string documentId, T document, CancellationToken ct = default)
    {
        await _client.UpdateAsync<T, T>(indexName, documentId,
            u => u.Doc(document), ct).ConfigureAwait(false);
    }

    /// <summary>Deletes the document identified by <paramref name="documentId"/> from the specified index.</summary>
    public async Task DeleteDocumentAsync(string indexName, string documentId, CancellationToken ct = default)
    {
        await _client.DeleteAsync<T>(documentId, d => d.Index(indexName), ct).ConfigureAwait(false);
    }

    /// <summary>Retrieves a document by its ID from the specified index, or returns <see langword="null"/> if not found.</summary>
    public async Task<T?> GetByIdAsync(string indexName, string documentId, CancellationToken ct = default)
    {
        var response = await _client.GetAsync<T>(indexName, documentId, ct).ConfigureAwait(false);
        return response.Found ? response.Source : null;
    }

    /// <summary>Executes a query-string search against the specified index (or the configured default) and returns paged, optionally sorted results.</summary>
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
