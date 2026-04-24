# Search

Full-text arama icin abstraction. Su an sadece Elasticsearch provider var.

## Paketler

| Paket | Hedef |
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

## Index Tanimlama

Elasticsearch mapping'i service-level'da degil, domain tarafinda — SpatNest
veya Elastic.Clients fluent API ile tanimlayin, ornegin startup migration
script'i:

```csharp
await client.Indices.CreateAsync<Product>(c => c
    .Index("products")
    .Mappings(m => m.Properties(p => p
        .Text(t => t.Name.Keyword())
        .Number(n => n.Price)
    )));
```

## Kullanim

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

## Index Stratejileri

- **Index-per-tenant**: `products-tenantA`, `products-tenantB`
- **Single index + filter**: `products` + `tenantId` query filter
- **Rollover**: zaman bazli (`logs-2026-04`, `logs-2026-05`)

## Performans

- Bulk index: `IndexBulkAsync` (su an abstraction'da yok — ekleme adayi)
- Query cache: Elasticsearch kendi query cache'i yonetir
- Refresh interval: production'da `1s` default — yuksek ingest rate icin `5s`
  veya `-1` (manuel refresh)

## Gelecek Planlar

- OpenSearch varyanti
- Vector search (k-NN) destegi
- Bulk operations API
