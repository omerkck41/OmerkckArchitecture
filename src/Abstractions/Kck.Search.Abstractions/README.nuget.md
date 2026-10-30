# Kck.Search.Abstractions

Full-text search abstractions with index management — `ISearchService<T>` for create/delete index, document CRUD, bulk indexing, and query operations backed by the Elasticsearch provider.

## Installation

```bash
dotnet add package Kck.Search.Abstractions
```

## Quick Start

```csharp
using Kck.Search.Abstractions;

// Program.cs — the provider (Kck.Search.Elasticsearch), one registration per document type
builder.Services.AddKckSearchElasticsearch<ProductDocument>(opt =>
{
    opt.ConnectionString = builder.Configuration["Elasticsearch:Uri"]!;
    opt.DefaultIndex = "products";
});

public sealed record ProductDocument(string Id, string Name, string Description);

// Use ISearchService<T> in a service
public class ProductSearchService(ISearchService<ProductDocument> search)
{
    public Task IndexAsync(ProductDocument doc, CancellationToken ct) =>
        search.IndexDocumentAsync("products", doc.Id, doc, ct);

    public async Task<IReadOnlyList<ProductDocument>> SearchAsync(string query, CancellationToken ct)
    {
        var result = await search.SearchAsync(new SearchRequest { Query = query, From = 0, Size = 20 }, ct);
        return result.Items;
    }

    public Task BulkIndexAsync(IEnumerable<ProductDocument> docs, CancellationToken ct) =>
        search.BulkIndexAsync("products", docs, ct);
}
```

## Configuration

| Property | Description | Default |
|---|---|---|
| `Search:Elasticsearch:Uri` | Elasticsearch node URI | `"http://localhost:9200"` |
| `Search:Elasticsearch:DefaultIndex` | Fallback index name | `"default"` |
| `Search:Elasticsearch:NumberOfShards` | Shards per created index | `1` |
| `Search:Elasticsearch:NumberOfReplicas` | Replicas per created index | `1` |

## Resources

- [Documentation](https://github.com/omerkck41/OmerkckArchitecture/blob/main/docs/providers/search.md)
- [CHANGELOG](https://github.com/omerkck41/OmerkckArchitecture/blob/main/CHANGELOG.md)
- [Source](https://github.com/omerkck41/OmerkckArchitecture)
