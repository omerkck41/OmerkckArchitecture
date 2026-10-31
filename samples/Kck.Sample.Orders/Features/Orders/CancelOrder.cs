using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Sample.Orders.Data;
using Kck.Sample.Orders.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kck.Sample.Orders.Features.Orders;

/// <summary>
/// DELETE /orders/{id}. A soft delete: the order and its lines are marked deleted (IsDeleted, DeletedDate) and hidden
/// from queries; the customer is untouched. The reserved stock is returned in the same transaction. A shipped order
/// cannot be cancelled (409), nor one that another request changed since it was read (409).
/// </summary>
public static class CancelOrder
{
    public static async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> HandleAsync(
        Guid id,
        IUnitOfWork unitOfWork,
        OrdersDbContext db,
        CancellationToken cancellationToken)
    {
        var orders = unitOfWork.Repository<Order, Guid>();
        var order = await orders.GetAsync(o => o.Id == id, cancellationToken: cancellationToken);
        if (order is null)
            return TypedResults.NotFound();

        if (!order.CanCancel)
            return TypedResults.Conflict(OrderEndpoints.Conflict("Order cannot be cancelled", $"The order is {order.Status}."));

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await orders.DeleteAsync(order, cancellationToken: cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            return TypedResults.Conflict(OrderEndpoints.ChangedConflict());
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
