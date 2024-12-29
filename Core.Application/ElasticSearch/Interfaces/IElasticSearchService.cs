namespace Core.Application.ElasticSearch.Interfaces;

public interface IElasticSearchService
{
    Task<bool> CreateIndexAsync(string indexName, int numberOfShards, int numberOfReplicas);
    Task<bool> InsertDocumentAsync<T>(string indexName, string documentId, T document) where T : class;
    Task<bool> UpdateDocumentAsync<T>(string indexName, string documentId, T document) where T : class;
    Task<bool> DeleteDocumentAsync(string indexName, string documentId);
    Task<T?> GetDocumentByIdAsync<T>(string indexName, string documentId) where T : class;
    Task<List<T>> SearchDocumentsAsync<T>(string indexName, string query, int from, int size) where T : class;
    Task<bool> BulkInsertAsync<T>(string indexName, List<T> documents) where T : class;
    Task<bool> DeleteIndexAsync(string indexName);
}