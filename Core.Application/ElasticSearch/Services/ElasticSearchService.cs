using Core.Application.ElasticSearch.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Application.ElasticSearch.Services;

public class ElasticSearchService : IElasticSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticSearchService> _logger;

    public ElasticSearchService(IConfiguration configuration, ILogger<ElasticSearchService> logger)
    {
        _logger = logger;

        var settings = configuration.GetSection("ElasticSearchSettings").Get<ElasticSearchSettings>()
                      ?? throw new InvalidOperationException("ElasticSearch settings are not configured.");

        var clientSettings = new ElasticsearchClientSettings(new Uri(settings.ConnectionString))
            .DefaultIndex(settings.DefaultIndex)
            .Authentication(new BasicAuthentication(settings.Username, settings.Password));

        _client = new ElasticsearchClient(clientSettings);
    }

    public async Task<bool> CreateIndexAsync(string indexName, int numberOfShards, int numberOfReplicas)
    {
        try
        {
            var response = await _client.Indices.CreateAsync(indexName, c => c
                .Settings(s => s
                    .NumberOfShards(numberOfShards)
                    .NumberOfReplicas(numberOfReplicas)));

            return response.Acknowledged;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index {IndexName}", indexName);
            throw;
        }
    }

    public async Task<bool> CreateIndexIfNotExistsAsync(string indexName, int numberOfShards, int numberOfReplicas)
    {
        var existsResponse = await _client.Indices.ExistsAsync(indexName);
        if (!existsResponse.Exists)
        {
            return await CreateIndexAsync(indexName, numberOfShards, numberOfReplicas);
        }
        return true;
    }

    public async Task<bool> InsertDocumentAsync<T>(string indexName, string documentId, T document) where T : class
    {
        try
        {
            var response = await _client.IndexAsync(document, i => i.Index(indexName).Id(documentId));
            return response.IsValidResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting document {DocumentId} in index {IndexName}", documentId, indexName);
            throw;
        }
    }

    public async Task<bool> UpdateDocumentAsync<T>(string indexName, string documentId, T document) where T : class
    {
        try
        {
            var response = await _client.UpdateAsync<T, T>(indexName, documentId, u => u.Doc(document));
            return response.IsValidResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {DocumentId} in index {IndexName}", documentId, indexName);
            throw;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string indexName, string documentId)
    {
        try
        {
            var response = await _client.DeleteAsync<object>(indexName, documentId);
            return response.IsValidResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId} from index {IndexName}", documentId, indexName);
            throw;
        }
    }

    public async Task<T?> GetDocumentByIdAsync<T>(string indexName, string documentId) where T : class
    {
        try
        {
            var response = await _client.GetAsync<T>(indexName, documentId);
            return response.Found ? response.Source : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document {DocumentId} from index {IndexName}", documentId, indexName);
            throw;
        }
    }

    public async Task<List<T>> SearchDocumentsAsync<T>(string indexName, string query, int from, int size, string? sortField = null, bool isAscending = true) where T : class
    {
        try
        {
            var searchDescriptor = new SearchRequestDescriptor<T>(indexName)
                .From(from)
                .Size(size)
                .Query(q => q.QueryString(qs => qs.Query(query)));

            if (!string.IsNullOrEmpty(sortField))
            {
                searchDescriptor.Sort(s => s.Field(sortField, new FieldSort { Order = isAscending ? SortOrder.Asc : SortOrder.Desc }));
            }

            var response = await _client.SearchAsync<T>(searchDescriptor);
            return response.Documents.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents in index {IndexName}", indexName);
            throw;
        }
    }

    public async Task<bool> BulkInsertAsync<T>(string indexName, List<T> documents) where T : class
    {
        try
        {
            var response = await _client.BulkAsync(b => b.Index(indexName).IndexMany(documents));
            return response.IsValidResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk inserting documents in index {IndexName}", indexName);
            throw;
        }
    }

    public async Task<bool> DeleteIndexAsync(string indexName)
    {
        try
        {
            var response = await _client.Indices.DeleteAsync(indexName);
            return response.Acknowledged;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting index {IndexName}", indexName);
            throw;
        }
    }
}