using Kck.Persistence.Abstractions.UnitOfWork;
using KckWebApiTemplate.Api.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KckWebApiTemplate.Api.Orders;

/// <summary>
/// DELETE /orders/{id}. A soft delete: the order and its lines are marked deleted (IsDeleted, DeletedDate) and hidden
/// from queries; the customer is untouched. The reserved stock is returned in the same transaction. Pass
/// <c>permanent: true</c> to DeleteAsync to remove rows instead.
/// </summary>
public static class CancelOrder
{
    public static async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> HandleAsync(
        Guid id,
        IUnitOfWork unitOfWork,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var orders = unitOfWork.Repository<Order, Guid>();
        var order = await orders.GetAsync(o => o.Id == id, cancellationToken: cancellationToken);
        if (order is null)
            return TypedResults.NotFound();

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await orders.DeleteAsync(order, cancellationToken: cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        catch (DbUpdateConcurrencyException) // another request cancelled the order since it was read
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            return TypedResults.Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Order changed",
                Detail = "Another request changed the order since it was read; read it again and retry.",
            });
        }

        // Only the request whose save succeeded gets here, so the stock comes back once.
        foreach (var returned in order.Lines.GroupBy(l => l.Sku).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var quantity = returned.Sum(l => l.Quantity);
            await db.Products
                .Where(p => p.Id == returned.Key)
                .ExecuteUpdateAsync(set => set.SetProperty(p => p.Stock, p => p.Stock + quantity), cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return TypedResults.NoContent();
    }
}
