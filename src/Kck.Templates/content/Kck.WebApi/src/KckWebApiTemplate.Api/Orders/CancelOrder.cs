using Kck.Persistence.Abstractions.UnitOfWork;
using Microsoft.AspNetCore.Http.HttpResults;

namespace KckWebApiTemplate.Api.Orders;

/// <summary>
/// DELETE /orders/{id}. A soft delete: the order and its lines are marked deleted (IsDeleted, DeletedDate) and hidden
/// from queries; the customer is untouched. Pass <c>permanent: true</c> to DeleteAsync to remove rows instead.
/// </summary>
public static class CancelOrder
{
    public static async Task<Results<NoContent, NotFound>> HandleAsync(
        Guid id,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var orders = unitOfWork.Repository<Order, Guid>();
        var order = await orders.GetAsync(o => o.Id == id, cancellationToken: cancellationToken);
        if (order is null)
            return TypedResults.NotFound();

        await orders.DeleteAsync(order, cancellationToken: cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        return TypedResults.NoContent();
    }
}
