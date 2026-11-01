# Search

Abstraction for full-text search. Currently only an Elasticsearch provider exists.

## Packages

| Package | Purpose |
|---|---|
| `Kck.Search.Abstractions` | `ISearchService<T>`, `SearchRequest`, `SearchResult<T>` |
| `Kck.Search.Elasticsearch` | Elasticsearch 9.x (Elastic.Clients.Elasticsearch) |

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

`SearchRequest.Query` uses the Elasticsearch query-string syntax, in which a stray `(` or `:` is a syntax error. Set
`UseSimpleQueryString = true` for end-user input: the query then runs as `simple_query_string`, which never throws on
syntax errors and ignores unsupported operators. Simple mode enables only `+` (AND), `|` (OR), `-` (NOT), `"` phrases,
`( )` grouping, whitespace and `\` escaping; the prefix (`*`), fuzzy and slop (`~N`) operators are off, so `alpha*`
searches for the term `alpha` and does not match `alphabet` (the default mode does).

`SearchRequest.IndexName` overrides `DefaultIndex`. `BulkIndexAsync`, `UpdateDocumentAsync`, `GetByIdAsync` and the
index operations (`CreateIndexAsync`, `IndexExistsAsync`, `DeleteIndexAsync`) complete the API.

## Errors

The write operations (`CreateIndexAsync`, `IndexDocumentAsync`, `BulkIndexAsync`, `UpdateDocumentAsync`,
`DeleteDocumentAsync`, `DeleteIndexAsync`) and `SearchAsync` throw `InvalidOperationException` when Elasticsearch
rejects the request, for example an invalid index name, an update of a document that does not exist, a search on a
missing index or a query-string syntax error. The message names the operation, the index, the HTTP status and the
server's error type and reason (the full request/response dump is logged at Debug level only). Deleting a missing
document or index does not throw, `GetByIdAsync` returns `null` for a missing document and `IndexExistsAsync` returns
`false` for a missing index; any other rejection of these two throws as well. Before 5.0, `IndexDocumentAsync` and
`BulkIndexAsync` only logged a warning, the other writes ignored the response and `SearchAsync` returned an empty
result.

`BulkIndexAsync` with an empty collection sends no request. A bulk request is not atomic: when some items fail, the
items Elasticsearch accepted stay indexed and the exception gives the number of failed items and the first reason.
Re-running the batch is safe, because documents with the same id are overwritten.

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
