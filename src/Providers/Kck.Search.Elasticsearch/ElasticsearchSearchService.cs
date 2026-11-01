using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Elastic.Transport.Products.Elasticsearch;
using Kck.Search.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kck.Search.Elasticsearch;

/// <summary>
/// Elasticsearch-backed implementation of <see cref="ISearchService{T}"/> that supports index management, document CRUD, bulk indexing, and full-text search for documents of type <typeparamref name="T"/>.
/// </summary>
public sealed partial class ElasticsearchSearchService<T> : ISearchService<T>, IDisposable where T : class
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchClientSettings _clientSettings;
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
        _clientSettings = CreateSettings(_options);
        _client = new ElasticsearchClient(_clientSettings);
    }

    /// <inheritdoc/>
    public void Dispose() => ((IDisposable?)_clientSettings)?.Dispose();

    [LoggerMessage(Level = LogLevel.Error, Message = "Elasticsearch did not acknowledge the creation of index {IndexName}")]
    private static partial void LogIndexCreateNotAcknowledged(ILogger logger, string indexName);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Elasticsearch {Operation} failed for index {IndexName}, document {DocumentId}: HTTP {StatusCode}, {ErrorType}: {ErrorReason}")]
    private static partial void LogRequestFailed(
        ILogger logger, string operation, string indexName, string? documentId, int? statusCode, string? errorType,
        string? errorReason);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "Elasticsearch {Operation} failed for index {IndexName}; debug information: {DebugInfo}")]
    private static partial void LogRequestFailedDebug(ILogger logger, string operation, string indexName, string debugInfo);

    // Simple mode operators: boolean logic, phrases, grouping and escaping only. PREFIX, FUZZY, SLOP and NEAR are left
    // out because they expand a term into many (expensive, surprising for end-user input).
    private const Elastic.Clients.Elasticsearch.QueryDsl.SimpleQueryStringFlags SafeSimpleQueryFlags =
        Elastic.Clients.Elasticsearch.QueryDsl.SimpleQueryStringFlags.And
        | Elastic.Clients.Elasticsearch.QueryDsl.SimpleQueryStringFlags.Or
        | Elastic.Clients.Elasticsearch.QueryDsl.SimpleQueryStringFlags.Not
        | Elastic.Clients.Elasticsearch.QueryDsl.SimpleQueryStringFlags.Phrase
        | Elastic.Clients.Elasticsearch.QueryDsl.SimpleQueryStringFlags.Precedence
        | Elastic.Clients.Elasticsearch.QueryDsl.SimpleQueryStringFlags.Whitespace
        | Elastic.Clients.Elasticsearch.QueryDsl.SimpleQueryStringFlags.Escape;

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Elasticsearch bulk index failed for {ErrorCount} of {ItemCount} documents in index {IndexName}; first error: {FirstReason}")]
    private static partial void LogBulkIndexFailed(
        ILogger logger, string indexName, int errorCount, int itemCount, string? firstReason);

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

    /// <summary>
    /// Creates a new Elasticsearch index with the shard and replica counts from options, throwing
    /// <see cref="InvalidOperationException"/> when Elasticsearch rejects the request (with the server's error type and
    /// reason) or does not acknowledge the creation.
    /// </summary>
    public async Task CreateIndexAsync(string indexName, CancellationToken ct = default)
    {
        var response = await _client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .NumberOfShards(_options.NumberOfShards)
                .NumberOfReplicas(_options.NumberOfReplicas)), ct).ConfigureAwait(false);

        EnsureValid(response, "create index", indexName);

        if (!response.Acknowledged)
        {
            LogIndexCreateNotAcknowledged(_logger, indexName);
            throw new InvalidOperationException(
                $"Elasticsearch did not acknowledge the creation of index '{indexName}'.");
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> if an index with the given name exists in the Elasticsearch cluster and
    /// <see langword="false"/> when it does not; any other rejection throws <see cref="InvalidOperationException"/>.
    /// </summary>
    public async Task<bool> IndexExistsAsync(string indexName, CancellationToken ct = default)
    {
        var response = await _client.Indices.ExistsAsync(indexName, ct).ConfigureAwait(false);

        if (IsNotFound(response))
            return false;

        EnsureValid(response, "index exists", indexName);
        return response.Exists;
    }

    /// <summary>
    /// Deletes the specified index from Elasticsearch. A missing index is not an error; any other rejection throws
    /// <see cref="InvalidOperationException"/> with the server's response.
    /// </summary>
    public async Task DeleteIndexAsync(string indexName, CancellationToken ct = default)
    {
        var response = await _client.Indices.DeleteAsync(indexName, ct).ConfigureAwait(false);

        if (!IsNotFound(response))
            EnsureValid(response, "delete index", indexName);
    }

    /// <summary>
    /// Indexes or replaces a single document with the given <paramref name="documentId"/> in the specified index,
    /// throwing <see cref="InvalidOperationException"/> with the server's response when Elasticsearch rejects it.
    /// </summary>
    public async Task IndexDocumentAsync(string indexName, string documentId, T document, CancellationToken ct = default)
    {
        var response = await _client.IndexAsync(document, i => i
            .Index(indexName)
            .Id(documentId), ct).ConfigureAwait(false);

        EnsureValid(response, "index", indexName, documentId);
    }

    /// <summary>
    /// Sends all <paramref name="documents"/> to Elasticsearch in a single bulk request, throwing
    /// <see cref="InvalidOperationException"/> when the request or any of its items is rejected. An empty collection
    /// sends no request. A rejected item does not roll back the others: the items Elasticsearch accepted stay indexed,
    /// the exception names the failed count and the first reason, and re-running the batch overwrites documents with
    /// the same id.
    /// </summary>
    public async Task BulkIndexAsync(string indexName, IEnumerable<T> documents, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var batch = documents as IReadOnlyCollection<T> ?? documents.ToList();
        if (batch.Count == 0)
            return;

        var response = await _client.BulkAsync(b => b
            .Index(indexName)
            .IndexMany(batch), ct).ConfigureAwait(false);

        // Item failures are checked first: they carry the per-document reason, and the request itself can be valid.
        if (response.Errors)
        {
            var failed = response.ItemsWithErrors.ToList();
            var firstReason = failed.FirstOrDefault()?.Error?.Reason;
            LogBulkIndexFailed(_logger, indexName, failed.Count, response.Items.Count, firstReason);
            throw new InvalidOperationException(
                $"Elasticsearch bulk index failed for {failed.Count} of {response.Items.Count} documents in index " +
                $"'{indexName}'; first error: {firstReason}");
        }

        EnsureValid(response, "bulk index", indexName);
    }

    /// <summary>
    /// Partially updates the document identified by <paramref name="documentId"/> with the fields present in
    /// <paramref name="document"/>, throwing <see cref="InvalidOperationException"/> with the server's response when
    /// Elasticsearch rejects it (for example when the document does not exist).
    /// </summary>
    public async Task UpdateDocumentAsync(string indexName, string documentId, T document, CancellationToken ct = default)
    {
        var response = await _client.UpdateAsync<T, T>(indexName, documentId,
            u => u.Doc(document), ct).ConfigureAwait(false);

        EnsureValid(response, "update", indexName, documentId);
    }

    /// <summary>
    /// Deletes the document identified by <paramref name="documentId"/> from the specified index. A missing document
    /// is not an error; any other rejection throws <see cref="InvalidOperationException"/> with the server's response.
    /// </summary>
    public async Task DeleteDocumentAsync(string indexName, string documentId, CancellationToken ct = default)
    {
        var response = await _client.DeleteAsync<T>(documentId, d => d.Index(indexName), ct).ConfigureAwait(false);

        if (!IsNotFound(response))
            EnsureValid(response, "delete", indexName, documentId);
    }

    private static bool IsNotFound(ElasticsearchResponse response) =>
        response.ApiCallDetails?.HttpStatusCode == 404;

    // The exception and the Error log carry the HTTP status and the server's error type and reason; the full
    // DebugInformation (request/response dump) goes to the Debug log only.
    private void EnsureValid(ElasticsearchResponse response, string operation, string indexName, string? documentId = null)
    {
        if (response.IsValidResponse)
            return;

        var status = response.ApiCallDetails?.HttpStatusCode;
        var serverError = response.ElasticsearchServerError?.Error;
        var detail = serverError is not null
            ? $"{serverError.Type}: {serverError.Reason}"
            : response.DebugInformation;

        LogRequestFailed(_logger, operation, indexName, documentId, status, serverError?.Type, serverError?.Reason);
        if (_logger.IsEnabled(LogLevel.Debug))
            LogRequestFailedDebug(_logger, operation, indexName, response.DebugInformation);
        response.TryGetOriginalException(out var innerException);
        throw new InvalidOperationException(
            $"Elasticsearch {operation} failed for index '{indexName}'" +
            (documentId is null ? "" : $", document '{documentId}'") +
            $" (HTTP {(status is null ? "n/a" : status.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))}): {detail}",
            innerException);
    }

    /// <summary>
    /// Retrieves a document by its ID from the specified index, or returns <see langword="null"/> if it is not found
    /// (missing document or missing index). Any other rejection throws <see cref="InvalidOperationException"/> with
    /// the server's error type and reason.
    /// </summary>
    public async Task<T?> GetByIdAsync(string indexName, string documentId, CancellationToken ct = default)
    {
        var response = await _client.GetAsync<T>(indexName, documentId, ct).ConfigureAwait(false);

        if (IsNotFound(response))
            return null;

        EnsureValid(response, "get", indexName, documentId);
        return response.Found ? response.Source : null;
    }

    /// <summary>
    /// Executes a query-string search (or a <c>simple_query_string</c> search when
    /// <see cref="ElasticsearchOptions.UseSimpleQueryString"/> is set) against the specified index (or the configured
    /// default) and returns paged, optionally sorted results. Simple mode enables only the AND, OR, NOT, phrase,
    /// precedence, whitespace and escape operators: no prefix (<c>*</c>), fuzzy or slop (<c>~N</c>) expansion.
    /// A rejected search (missing index, query syntax error in the default mode) throws
    /// <see cref="InvalidOperationException"/> with the HTTP status and the server's error type and reason instead of
    /// returning an empty result.
    /// </summary>
    public async Task<SearchResult<T>> SearchAsync(Abstractions.SearchRequest request, CancellationToken ct = default)
    {
        var indexName = request.IndexName ?? _options.DefaultIndex
            ?? throw new InvalidOperationException("No index name specified and no default index configured.");

        var searchDescriptor = new SearchRequestDescriptor<T>(indexName)
            .From(request.From)
            .Size(request.Size)
            .Query(q =>
            {
                if (_options.UseSimpleQueryString)
                    q.SimpleQueryString(s => s.Query(request.Query).Flags(SafeSimpleQueryFlags));
                else
                    q.QueryString(qs => qs.Query(request.Query));
            });

        if (!string.IsNullOrEmpty(request.SortField))
        {
            searchDescriptor.Sort(s => s.Field(f => f
                .Field(request.SortField!)
                .Order(request.SortAscending ? SortOrder.Asc : SortOrder.Desc)));
        }

        var response = await _client.SearchAsync<T>(searchDescriptor, ct).ConfigureAwait(false);
        EnsureValid(response, "search", indexName);

        return new SearchResult<T>
        {
            Items = response.Documents.ToList(),
            TotalCount = response.Total
        };
    }
}
