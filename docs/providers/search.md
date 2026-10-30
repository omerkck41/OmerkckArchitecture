# Search

Abstraction for full-text search. Currently only an Elasticsearch provider exists.

## Packages

| Package | Purpose |
|---|---|
| `Kck.Search.Abstractions` | `ISearchService<T>`, `SearchRequest`, `SearchResult<T>` |
| `Kck.Search.Elasticsearch` | Elasticsearch 8.x (Elastic.Clients.Elasticsearch) |

## Setup

One registration per document type:

```csharp
services.AddKckSearchElasticsearch<Product>(opt =>
{
    opt.ConnectionString = builder.Configuration["Elasticsearch:Uri"]!;
    opt.Username = builder.Configuration["Elasticsearch:User"];
    opt.Password = builder.Configuration["Elasticsearch:Password"];
    opt.DefaultIndex = "products";
});
```

The options are validated at startup. `DisableSslVerification` exists for local clusters with self-signed
certificates; never set it in production.

## Usage

```csharp
using Kck.Search.Abstractions;

public class ProductSearchService(ISearchService<Product> search)
{
    public Task<SearchResult<Product>> FindAsync(string query, int page, CancellationToken ct) =>
        search.SearchAsync(new SearchRequest
        {
            Query = query,          // Elasticsearch query-string syntax, e.g. "name:phone AND price:<500"
            From = page * 20,
            Size = 20,
            SortField = "price",
            SortAscending = true
        }, ct);

    public Task IndexAsync(Product product, CancellationToken ct) =>
        search.IndexDocumentAsync("products", product.Id.ToString(), product, ct);

    public Task DeleteAsync(int id, CancellationToken ct) =>
        search.DeleteDocumentAsync("products", id.ToString(), ct);
}
```

`SearchRequest.IndexName` overrides `DefaultIndex`. `BulkIndexAsync`, `UpdateDocumentAsync`, `GetByIdAsync` and the
index operations (`CreateIndexAsync`, `IndexExistsAsync`, `DeleteIndexAsync`) complete the API.

## Defining an Index

`CreateIndexAsync(name)` creates an index with `NumberOfShards` and `NumberOfReplicas` and dynamic mapping. For an
explicit mapping, create the index yourself with the `Elastic.Clients.Elasticsearch` client (for example in a startup
step) before indexing documents.

## Result

```csharp
public sealed class SearchResult<T> where T : class
{
    public required IReadOnlyList<T> Items { get; init; }
    public long TotalCount { get; init; }
}
```

## Index Strategies

- **Index-per-tenant**: `products-tenantA`, `products-tenantB`
- **Single index + filter**: `products` + a `tenantId` query filter
- **Rollover**: time-based (`logs-2026-04`, `logs-2026-05`)

## Performance

- Bulk index: `BulkIndexAsync(index, documents)`
- Query cache: Elasticsearch manages its own query cache
- Refresh interval: default `1s` in production. For a high ingest rate, use `5s`
  or `-1` (manual refresh)

## Planned Work

- OpenSearch variant
- Vector search (k-NN) support
