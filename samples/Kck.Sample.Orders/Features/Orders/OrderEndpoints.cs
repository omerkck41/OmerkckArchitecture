using Kck.Sample.Orders.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Kck.Sample.Orders.Features.Orders;

public static class OrderEndpoints
{
    /// <summary>
    /// Maps the order endpoints; all of them require an authenticated caller. The sample has no ownership rules: any
    /// authenticated caller can read or change any order. A real application takes the customer from the token and
    /// filters every query by it.
    /// </summary>
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/orders").WithTags("Orders").RequireAuthorization();
        orders.MapPost("/", PlaceOrder.HandleAsync).WithName(nameof(PlaceOrder));
        orders.MapGet("/{id:guid}", GetOrder.HandleAsync).WithName(nameof(GetOrder));
        orders.MapGet("/", ListOrders.HandleAsync).WithName(nameof(ListOrders));
        orders.MapPost("/search", SearchOrders.HandleAsync).WithName(nameof(SearchOrders));
        orders.MapPost("/{id:guid}/pay", ChangeOrderStatus.PayAsync).WithName("PayOrder");
        orders.MapPost("/{id:guid}/ship", ChangeOrderStatus.ShipAsync).WithName("ShipOrder");
        orders.MapDelete("/{id:guid}", CancelOrder.HandleAsync).WithName(nameof(CancelOrder));
        return app;
    }

    internal static ProblemDetails Conflict(string title, string detail) => new()
    {
        Status = StatusCodes.Status409Conflict,
        Title = title,
        Detail = detail,
    };

    /// <summary>The order was paid, shipped or cancelled by another request since this one read it.</summary>
    internal static ProblemDetails ChangedConflict() =>
        Conflict("Order changed", "Another request changed the order since it was read; read it again and retry.");
}

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal Total,
    bool IsDeleted,
    DateTime CreatedDate,
    DateTime? DeletedDate,
    IReadOnlyList<OrderLineResponse> Lines)
{
    public static OrderResponse From(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Status.ToString(),
        order.Total,
        order.IsDeleted,
        order.CreatedDate,
        order.DeletedDate,
        [.. order.Lines.Select(l => new OrderLineResponse(l.Sku, l.Quantity, l.UnitPrice, l.IsDeleted))]);
}

public sealed record OrderLineResponse(string Sku, int Quantity, decimal UnitPrice, bool IsDeleted);

public sealed record OrderSummary(Guid Id, Guid CustomerId, string Status, int LineCount, DateTime CreatedDate);
