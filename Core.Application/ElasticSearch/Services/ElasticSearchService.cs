using Core.Application.ElasticSearch.Interfaces;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;

namespace Core.Application.ElasticSearch.Services;

public class ElasticSearchService : IElasticSearchService
{
    private readonly ElasticsearchClient _client;

    public ElasticSearchService(IConfiguration configuration)
    {
        var settings = configuration.GetSection("ElasticSearchSettings").Get<ElasticSearchSettings>()
                      ?? throw new InvalidOperationException("ElasticSearch settings are not configured.");

        var clientSettings = new ElasticsearchClientSettings(new Uri(settings.ConnectionString))
            .DefaultIndex(settings.DefaultIndex);

        _client = new ElasticsearchClient(clientSettings);
    }

    public async Task<bool> CreateIndexAsync(string indexName, int numberOfShards, int numberOfReplicas)
    {
        var response = await _client.Indices.CreateAsync(indexName, c => c
                .Settings(s => s
                    .NumberOfShards(numberOfShards)
                    .NumberOfReplicas(numberOfReplicas)));

        return response.Acknowledged;
    }

    public async Task<bool> InsertDocumentAsync<T>(string indexName, string documentId, T document) where T : class
    {
        var response = await _client.IndexAsync(document, i => i.Index(indexName).Id(documentId));
        return response.IsValidResponse;
    }

    public async Task<bool> UpdateDocumentAsync<T>(string indexName, string documentId, T document) where T : class
    {
        var response = await _client.UpdateAsync(new UpdateRequest<T, T>(indexName, documentId) { Doc = document });
        return response.IsValidResponse;
    }

    public async Task<bool> DeleteDocumentAsync(string indexName, string documentId)
    {
        var response = await _client.DeleteAsync(new DeleteRequest(indexName, documentId));
        return response.IsValidResponse;
    }

    public async Task<T?> GetDocumentByIdAsync<T>(string indexName, string documentId) where T : class
    {
        var response = await _client.GetAsync<T>(documentId, g => g.Index(indexName));
        return response.Found ? response.Source : null;
    }

    public async Task<List<T>> SearchDocumentsAsync<T>(string indexName, string query, int from, int size) where T : class
    {
        var response = await _client.SearchAsync<T>(s => s
                .Index(indexName)
                .From(from)
                .Size(size)
                .Query(q => q.QueryString(d => d.Query(query))));

        return response.Documents.ToList();
    }

    public async Task<bool> BulkInsertAsync<T>(string indexName, List<T> documents) where T : class
    {
        var response = await _client.BulkAsync(b => b.Index(indexName).IndexMany(documents));
        return response.IsValidResponse;
    }

    public async Task<bool> DeleteIndexAsync(string indexName)
    {
        var response = await _client.Indices.DeleteAsync(indexName);
        return response.Acknowledged;
    }
}