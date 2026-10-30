# Search

Abstraction for full-text search. Currently only an Elasticsearch provider exists.

## Packages

| Package | Purpose |
|---|---|
| `Kck.Search.Abstractions` | `ISearchService`, `SearchQuery`, `SearchResult<T>` |
| `Kck.Search.Elasticsearch` | Elasticsearch 8.x (Elastic.Clients.Elasticsearch) |

## Setup

```csharp
services.AddKckSearchElasticsearch(opt =>
{
    opt.Uri = builder.Configuration["Elasticsearch:Uri"]!;
    opt.UserName = builder.Configuration["Elasticsearch:User"];
    opt.Password = builder.Configuration["Elasticsearch:Password"];
    opt.DefaultIndex = "products";
});
```

## Defining an Index

The Elasticsearch mapping lives on the domain side, not in the service layer. Define it with SpatNest
or the Elastic.Clients fluent API, for example in a startup migration
script:

```csharp
await client.Indices.CreateAsync<Product>(c => c
    .Index("products")
    .Mappings(m => m.Properties(p => p
        .Text(t => t.Name.Keyword())
        .Number(n => n.Price)
    )));
```

## Usage

```csharp
public class ProductSearchService(ISearchService search)
{
    public async Task<SearchResult<Product>> FindAsync(string query, CancellationToken ct)
    {
        return await search.SearchAsync<Product>(new SearchQuery
        {
            Index = "products",
            Query = query,
            From = 0,
            Size = 20,
            Fields = ["name", "description"]
        }, ct);
    }

    public Task IndexAsync(Product product, CancellationToken ct)
        => search.IndexAsync(product, $"product:{product.Id}", "products", ct);

    public Task DeleteAsync(int id, CancellationToken ct)
        => search.DeleteAsync($"product:{id}", "products", ct);
}
```

## Result

```csharp
public record SearchResult<T>(
    IReadOnlyList<T> Items,
    long TotalCount,
    long TookMs);
```

## Index Strategies

- **Index-per-tenant**: `products-tenantA`, `products-tenantB`
- **Single index + filter**: `products` + a `tenantId` query filter
- **Rollover**: time-based (`logs-2026-04`, `logs-2026-05`)

## Performance

- Bulk index: `IndexBulkAsync` (not in the abstraction yet; candidate to add)
- Query cache: Elasticsearch manages its own query cache
- Refresh interval: default `1s` in production. For a high ingest rate, use `5s`
  or `-1` (manual refresh)

## Planned Work

- OpenSearch variant
- Vector search (k-NN) support
- Bulk operations API
