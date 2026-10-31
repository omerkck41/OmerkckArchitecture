using Kck.Core.Abstractions.Paging;
using Kck.Persistence.Abstractions.Dynamic;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.Security;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Sample.Orders.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Kck.Sample.Orders.Features.Orders;

/// <summary>
/// The order properties a client may filter and sort on. Anything else (IsDeleted, audit fields) is rejected before a
/// query is built; register one whitelist per entity that takes dynamic queries.
/// </summary>
public sealed class OrderFilterWhitelist : FilterPropertyWhitelist<Order>
{
    public OrderFilterWhitelist()
    {
        Allow(o => o.Status);
        Allow(o => o.CustomerId);
        Allow(o => o.CreatedDate);
        Allow(o => o.Id);
    }
}

/// <summary>
/// POST /orders/search?index=0&amp;size=10 with a <see cref="DynamicQuery"/> body, for example
/// <c>{"filter":{"field":"Status","operator":"eq","value":"Paid"},"sort":[{"field":"CreatedDate","dir":"desc"}]}</c>.
/// A field outside <see cref="OrderFilterWhitelist"/> returns 400. Without a sort the newest orders come first, as in
/// GET /orders, and any sort ends with the id: paging over an order that is not unique can repeat or skip rows.
/// </summary>
public static class SearchOrders
{
    public static async Task<Results<Ok<IPaginate<OrderSummary>>, ValidationProblem>> HandleAsync(
        DynamicQuery query,
        int? index,
        int? size,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var sort = query.Sort.ToList();
        if (sort.Count == 0)
            sort = [new Sort(nameof(Order.CreatedDate), "desc"), new Sort(nameof(Order.Id), "desc")];
        else if (!sort[^1].Field.Equals(nameof(Order.Id), StringComparison.OrdinalIgnoreCase))
            sort.Add(new Sort(nameof(Order.Id), "asc"));
        query.Sort = sort;

        IPaginate<Order> page;
        try
        {
            page = await unitOfWork.Repository<Order, Guid>().GetListByDynamicAsync(
                query,
                QueryOptions.None,
                index: ListOrders.PageIndex(index),
                size: ListOrders.PageSize(size),
                cancellationToken: cancellationToken);
        }
        catch (ArgumentException ex) // a field outside the whitelist or an unknown operator
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["query"] = [ex.Message] });
        }

        return TypedResults.Ok<IPaginate<OrderSummary>>(new Paginate<OrderSummary>
        {
            From = page.From,
            Index = page.Index,
            Size = page.Size,
            Count = page.Count,
            Pages = page.Pages,
            HasPrevious = page.HasPrevious,
            HasNext = page.HasNext,
            Items = [.. page.Items.Select(o => new OrderSummary(o.Id, o.CustomerId, o.Status.ToString(), o.Lines.Count, o.CreatedDate))],
        });
    }
}
