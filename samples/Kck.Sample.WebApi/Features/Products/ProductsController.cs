using Kck.AspNetCore.Controllers;
using Kck.Sample.WebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kck.Sample.WebApi.Features.Products;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(AppDbContext db, ILogger<ProductsController> logger) : KckApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        logger.LogInformation("Fetching all products");
        var products = await db.Products.AsNoTracking().ToListAsync(ct);
        return ApiSuccess(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct);
        if (product is null) return ApiFail("Product not found", 404);

        return ApiSuccess(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = new Product
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name,
            Category = request.Category,
            Price = request.Price
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Product created: {ProductId}", product.Id);
        return ApiSuccess(product, "Product created", 201);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct);
        if (product is null) return ApiFail("Product not found", 404);

        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);

        return ApiSuccess<object?>(null, statusCode: 204);
    }
}

public sealed record CreateProductRequest(string Name, string Category, decimal Price);
