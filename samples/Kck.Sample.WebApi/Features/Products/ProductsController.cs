using Kck.AspNetCore.Controllers;
using Kck.Sample.WebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kck.Sample.WebApi.Features.Products;

// Mutation endpoints require authentication. Reads are public for demo browsability.
// To run this sample end-to-end, wire an auth scheme (e.g. Kck.Security.Jwt) and
// app.UseAuthentication()/UseAuthorization() in Program.cs.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed partial class ProductsController(AppDbContext db, ILogger<ProductsController> logger) : KckApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        LogFetchingAllProducts(logger);
        var products = await db.Products.AsNoTracking().ToListAsync(ct);
        return ApiSuccess(products);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
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

        LogProductCreated(logger, product.Id);
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

public sealed partial class ProductsController
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Fetching all products")]
    private static partial void LogFetchingAllProducts(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product created: {ProductId}")]
    private static partial void LogProductCreated(ILogger logger, Guid productId);
}
