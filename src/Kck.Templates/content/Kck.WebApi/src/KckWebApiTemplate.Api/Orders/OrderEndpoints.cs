namespace KckWebApiTemplate.Api.Orders;

public static class OrderEndpoints
{
    /// <summary>
    /// Maps the order endpoints; all of them require an authenticated caller. The sample has no ownership rules: any
    /// authenticated caller can read, place or cancel any order. Before production, take the customer from the token
    /// (for example the <c>sub</c> claim) instead of the request body and filter every query by it (README.md).
    /// </summary>
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/orders").WithTags("Orders").RequireAuthorization();
        orders.MapPost("/", PlaceOrder.HandleAsync).WithName(nameof(PlaceOrder));
        orders.MapGet("/{id:guid}", GetOrder.HandleAsync).WithName(nameof(GetOrder));
        orders.MapGet("/", ListOrders.HandleAsync).WithName(nameof(ListOrders));
        orders.MapDelete("/{id:guid}", CancelOrder.HandleAsync).WithName(nameof(CancelOrder));
        return app;
    }
}

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    decimal Total,
    bool IsDeleted,
    DateTime CreatedDate,
    DateTime? DeletedDate,
    IReadOnlyList<OrderLineResponse> Lines)
{
    public static OrderResponse From(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Total,
        order.IsDeleted,
        order.CreatedDate,
        order.DeletedDate,
        [.. order.Lines.Select(l => new OrderLineResponse(l.Sku, l.Quantity, l.UnitPrice, l.IsDeleted))]);
}

public sealed record OrderLineResponse(string Sku, int Quantity, decimal UnitPrice, bool IsDeleted);
