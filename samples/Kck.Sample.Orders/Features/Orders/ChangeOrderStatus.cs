using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Sample.Orders.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kck.Sample.Orders.Features.Orders;

/// <summary>
/// POST /orders/{id}/pay and /orders/{id}/ship. A move the current status does not allow returns 409, and so does an
/// order that another request changed since it was read.
/// </summary>
public static class ChangeOrderStatus
{
    public static Task<Results<Ok<OrderResponse>, NotFound, Conflict<ProblemDetails>>> PayAsync(
        Guid id, IUnitOfWork unitOfWork, CancellationToken cancellationToken) =>
        MoveAsync(id, unitOfWork, order => order.TryPay(), "Only a placed order can be paid.", cancellationToken);

    public static Task<Results<Ok<OrderResponse>, NotFound, Conflict<ProblemDetails>>> ShipAsync(
        Guid id, IUnitOfWork unitOfWork, CancellationToken cancellationToken) =>
        MoveAsync(id, unitOfWork, order => order.TryShip(), "Only a paid order can be shipped.", cancellationToken);

    private static async Task<Results<Ok<OrderResponse>, NotFound, Conflict<ProblemDetails>>> MoveAsync(
        Guid id,
        IUnitOfWork unitOfWork,
        Func<Order, bool> move,
        string rule,
        CancellationToken cancellationToken)
    {
        var orders = unitOfWork.Repository<Order, Guid>();
        var order = await orders.GetAsync(o => o.Id == id, cancellationToken: cancellationToken);
        if (order is null)
            return TypedResults.NotFound();

        if (!move(order))
            return TypedResults.Conflict(OrderEndpoints.Conflict("Status change not allowed", $"{rule} The order is {order.Status}."));

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return TypedResults.Conflict(OrderEndpoints.ChangedConflict());
        }

        return TypedResults.Ok(OrderResponse.From(order));
    }
}
