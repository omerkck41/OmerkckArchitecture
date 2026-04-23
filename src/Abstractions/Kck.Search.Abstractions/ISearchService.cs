namespace Kck.Search.Abstractions;

/// <summary>
/// Provider-agnostic search service.
/// </summary>
public interface ISearchService<T> where T : class
{
    Task CreateIndexAsync(string indexName, CancellationToken ct = default);
    Task<bool> IndexExistsAsync(string indexName, CancellationToken ct = default);
    Task DeleteIndexAsync(string indexName, CancellationToken ct = default);

    Task IndexDocumentAsync(string indexName, string documentId, T document, CancellationToken ct = default);
    Task BulkIndexAsync(string indexName, IEnumerable<T> documents, CancellationToken ct = default);
    Task UpdateDocumentAsync(string indexName, string documentId, T document, CancellationToken ct = default);
    Task DeleteDocumentAsync(string indexName, string documentId, CancellationToken ct = default);

    Task<T?> GetByIdAsync(string indexName, string documentId, CancellationToken ct = default);
    Task<SearchResult<T>> SearchAsync(SearchRequest request, CancellationToken ct = default);
}
