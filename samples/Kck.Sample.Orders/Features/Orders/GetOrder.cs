using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Sample.Orders.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Kck.Sample.Orders.Features.Orders;

/// <summary>GET /orders/{id}. Cancelled (soft-deleted) orders are found only with <c>?includeDeleted=true</c>.</summary>
public static class GetOrder
{
    public static async Task<Results<Ok<OrderResponse>, NotFound>> HandleAsync(
        Guid id,
        bool? includeDeleted,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var options = includeDeleted == true ? QueryOptions.WithDeleted : QueryOptions.None;
        var order = await unitOfWork.Repository<Order, Guid>()
            .GetAsync(o => o.Id == id, options, cancellationToken: cancellationToken);

        return order is null ? TypedResults.NotFound() : TypedResults.Ok(OrderResponse.From(order));
    }
}
