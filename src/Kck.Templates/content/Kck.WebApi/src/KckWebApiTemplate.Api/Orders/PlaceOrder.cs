using Kck.Persistence.Abstractions.UnitOfWork;
using KckWebApiTemplate.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KckWebApiTemplate.Api.Orders;

public sealed record PlaceOrderRequest(Guid CustomerId, IReadOnlyList<PlaceOrderLine> Lines);

public sealed record PlaceOrderLine(string Sku, int Quantity);

/// <summary>
/// POST /orders. Saves the order and reserves stock in one transaction. If any product runs out of stock the
/// transaction rolls back: the order is not saved and <see cref="OrderPlaced"/> is discarded, never dispatched.
/// </summary>
public static class PlaceOrder
{
    public static async Task<Results<Created<OrderResponse>, ValidationProblem, Conflict<ProblemDetails>>> HandleAsync(
        PlaceOrderRequest request,
        IUnitOfWork unitOfWork,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
            return TypedResults.ValidationProblem(errors);

        if (!await db.Customers.AnyAsync(c => c.Id == request.CustomerId, cancellationToken))
            return TypedResults.ValidationProblem(Error(nameof(request.CustomerId), "Unknown customer."));

        var skus = request.Lines.Select(l => l.Sku).Distinct().ToList();
        var products = await db.Products.AsNoTracking()
            .Where(p => skus.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);
        var unknown = skus.Where(sku => !products.ContainsKey(sku)).ToList();
        if (unknown.Count > 0)
            return TypedResults.ValidationProblem(Error(nameof(request.Lines), $"Unknown SKU: {string.Join(", ", unknown)}."));

        var order = Order.Place(request.CustomerId, request.Lines.Select(l => OrderLine.For(products[l.Sku], l.Quantity)));

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await unitOfWork.Repository<Order, Guid>().AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken); // OrderPlaced waits for the commit

        // A fixed order (by SKU) so two concurrent orders never lock the same products in opposite order (deadlock).
        foreach (var requested in request.Lines.GroupBy(l => l.Sku).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var quantity = requested.Sum(l => l.Quantity);

            // Reserve atomically: another request may have taken the stock since it was read above.
            var reserved = await db.Products
                .Where(p => p.Id == requested.Key && p.Stock >= quantity)
                .ExecuteUpdateAsync(set => set.SetProperty(p => p.Stock, p => p.Stock - quantity), cancellationToken);
            if (reserved == 0)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                return TypedResults.Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Insufficient stock",
                    Detail = $"Not enough stock for {requested.Key}.",
                });
            }
        }

        await unitOfWork.CommitAsync(cancellationToken); // OrderPlaced is dispatched after this commit
        return TypedResults.Created($"/orders/{order.Id}", OrderResponse.From(order));
    }

    private const int MaxLines = 100;
    private const int MaxQuantity = 10_000;

    private static Dictionary<string, string[]> Validate(PlaceOrderRequest request)
    {
        if (request.Lines is not { Count: > 0 })
            return Error(nameof(request.Lines), "An order needs at least one line.");
        if (request.Lines.Count > MaxLines)
            return Error(nameof(request.Lines), $"An order can have at most {MaxLines} lines.");
        if (request.Lines.Any(l => l is null || string.IsNullOrWhiteSpace(l.Sku)))
            return Error(nameof(request.Lines), "Every line needs a SKU.");
        if (request.Lines.Any(l => l.Quantity is <= 0 or > MaxQuantity))
            return Error(nameof(request.Lines), $"Quantities must be between 1 and {MaxQuantity}.");
        return [];
    }

    private static Dictionary<string, string[]> Error(string field, string message) => new() { [field] = [message] };
}
