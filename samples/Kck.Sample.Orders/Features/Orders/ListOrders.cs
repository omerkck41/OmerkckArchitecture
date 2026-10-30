using Kck.Core.Abstractions.Paging;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Persistence.EntityFramework.Paging;
using Kck.Sample.Orders.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Kck.Sample.Orders.Features.Orders;

/// <summary>GET /orders?index=0&amp;size=10. Newest first; cancelled orders are not listed.</summary>
public static class ListOrders
{
    internal const int MaxPageSize = 100;

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
            .Select(o => new OrderSummary(o.Id, o.CustomerId, o.Status.ToString(), o.Lines.Count, o.CreatedDate))
            .ToPaginateAsync(PageIndex(index), PageSize(size), cancellationToken: cancellationToken);

        return TypedResults.Ok(page);
    }

    // index * size must not overflow.
    internal static int PageIndex(int? index) => Math.Clamp(index ?? 0, 0, int.MaxValue / MaxPageSize);

    internal static int PageSize(int? size) => Math.Clamp(size ?? 10, 1, MaxPageSize);
}
