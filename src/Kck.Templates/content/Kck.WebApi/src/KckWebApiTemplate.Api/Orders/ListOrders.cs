using Kck.Core.Abstractions.Paging;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework.Paging;
using Microsoft.AspNetCore.Http.HttpResults;

namespace KckWebApiTemplate.Api.Orders;

public sealed record OrderSummary(Guid Id, Guid CustomerId, int LineCount, DateTime CreatedDate);

/// <summary>GET /orders?index=0&amp;size=10. Newest first; cancelled orders are not listed.</summary>
public static class ListOrders
{
    private const int MaxPageSize = 100;

    public static async Task<Ok<IPaginate<OrderSummary>>> HandleAsync(
        int? index,
        int? size,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var page = await unitOfWork.Repository<Order, Guid>()
            .Query(QueryOptions.None)
            .OrderByDescending(o => o.CreatedDate)
            .ThenByDescending(o => o.Id)
            .Select(o => new OrderSummary(o.Id, o.CustomerId, o.Lines.Count, o.CreatedDate))
            .ToPaginateAsync(
                Math.Clamp(index ?? 0, 0, int.MaxValue / MaxPageSize), // index * size must not overflow
                Math.Clamp(size ?? 10, 1, MaxPageSize),
                cancellationToken: cancellationToken);

        return TypedResults.Ok(page);
    }
}
