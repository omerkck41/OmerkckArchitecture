using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Sample.Orders.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Kck.Sample.Orders.Features.Orders;

/// <summary>
/// DELETE /orders/{id}. A soft delete: the order and its lines are marked deleted (IsDeleted, DeletedDate) and hidden
/// from queries; the customer is untouched. A shipped order cannot be cancelled (409).
/// </summary>
public static class CancelOrder
{
    public static async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> HandleAsync(
        Guid id,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var orders = unitOfWork.Repository<Order, Guid>();
        var order = await orders.GetAsync(o => o.Id == id, cancellationToken: cancellationToken);
        if (order is null)
            return TypedResults.NotFound();

        if (!order.CanCancel)
            return TypedResults.Conflict(OrderEndpoints.Conflict("Order cannot be cancelled", $"The order is {order.Status}."));

        await orders.DeleteAsync(order, cancellationToken: cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        return TypedResults.NoContent();
    }
}
