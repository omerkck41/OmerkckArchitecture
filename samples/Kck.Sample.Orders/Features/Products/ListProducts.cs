using Kck.Caching.Abstractions;
using Kck.Sample.Orders.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Kck.Sample.Orders.Features.Products;

public sealed record CatalogueItem(string Sku, string Name, decimal UnitPrice);

/// <summary>
/// GET /products. The catalogue changes rarely, so it is cached for five minutes through <see cref="ICacheService"/>
/// (in memory here; Kck.Caching.Redis or Kck.Caching.Hybrid share it across instances). Stock is not part of the
/// cached item: it changes with every order.
/// </summary>
public static class ListProducts
{
    public const string CacheKey = "catalogue";

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products", HandleAsync).WithTags("Products").WithName(nameof(ListProducts)).RequireAuthorization();
        return app;
    }

    public static async Task<Ok<List<CatalogueItem>>> HandleAsync(
        ICacheService cache,
        OrdersDbContext db,
        CancellationToken cancellationToken)
    {
        var catalogue = await cache.GetOrSetAsync(
            CacheKey,
            () => db.Products.AsNoTracking()
                .OrderBy(p => p.Id)
                .Select(p => new CatalogueItem(p.Id, p.Name, p.UnitPrice))
                .ToListAsync(cancellationToken),
            CacheDuration,
            cancellationToken);

        return TypedResults.Ok(catalogue ?? []);
    }
}
