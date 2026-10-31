using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Kck.Core.Abstractions.Entities;
using Kck.Core.Abstractions.Paging;
using KckWebApiTemplate.Api.Data;
using KckWebApiTemplate.Api.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KckWebApiTemplate.Api.Tests;

public sealed class OrdersEndpointTests : IAsyncLifetime
{
    private readonly ApiFactory _factory = new();
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _client = _factory.CreateAuthenticatedClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => _factory.DisposeAsync().AsTask();

    private static PlaceOrderRequest Request(params PlaceOrderLine[] lines) => new(SampleData.DemoCustomerId, lines);

    private async Task<T> QueryAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    private Task<int> StockAsync(string sku) => QueryAsync(db => db.Products.Where(p => p.Id == sku).Select(p => p.Stock).SingleAsync());

    [Fact]
    public async Task Orders_WithoutAuthentication_Return401()
    {
        var response = await _factory.CreateClient().GetAsync("/orders");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PlaceOrder_ValidRequest_Returns201AndDispatchesOrderPlacedAfterCommit()
    {
        var response = await _client.PostAsJsonAsync("/orders", Request(new PlaceOrderLine("A-1", 2)));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order!.Total.Should().Be(20.00m);
        response.Headers.Location!.ToString().Should().Be($"/orders/{order.Id}");

        var recorded = _factory.Services.GetRequiredService<RecordedEvents>();
        recorded.Items.Should().ContainSingle().Which.Should().Be(
            new RecordedEvent(order.Id, CommittedWhenHandled: true));
        (await StockAsync("A-1")).Should().Be(98);
    }

    [Fact]
    public async Task PlaceOrder_InsufficientStock_Returns409AndPersistsNothing()
    {
        // B-2 has 5 in stock. The A-1 reservation made earlier in the same transaction is rolled back too.
        var response = await _client.PostAsJsonAsync("/orders", Request(new PlaceOrderLine("A-1", 2), new PlaceOrderLine("B-2", 6)));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await QueryAsync(db => db.Orders.IgnoreQueryFilters().CountAsync())).Should().Be(0);
        _factory.Services.GetRequiredService<RecordedEvents>().Items.Should().BeEmpty();
        (await StockAsync("A-1")).Should().Be(100);
        (await StockAsync("B-2")).Should().Be(5);
    }

    [Fact]
    public async Task PlaceOrder_UnknownSku_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/orders", Request(new PlaceOrderLine("NOPE", 1)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Unknown SKU: NOPE");
    }

    [Fact]
    public async Task CancelOrder_SoftDeletesOrderAndLines_KeepsCustomer()
    {
        var placed = await (await _client.PostAsJsonAsync("/orders", Request(new PlaceOrderLine("A-1", 1), new PlaceOrderLine("B-2", 1))))
            .Content.ReadFromJsonAsync<OrderResponse>();

        (await _client.DeleteAsync($"/orders/{placed!.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await _client.GetAsync($"/orders/{placed.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var cancelled = await _client.GetFromJsonAsync<OrderResponse>($"/orders/{placed.Id}?includeDeleted=true");
        cancelled!.IsDeleted.Should().BeTrue();
        cancelled.DeletedDate.Should().NotBeNull();
        cancelled.Lines.Should().HaveCount(2).And.OnlyContain(l => l.IsDeleted);
        (await QueryAsync(db => db.Customers.AnyAsync(c => c.Id == SampleData.DemoCustomerId))).Should().BeTrue();
    }

    [Fact]
    public async Task CancelOrder_ReturnsTheReservedStock()
    {
        var placed = await (await _client.PostAsJsonAsync("/orders", Request(new PlaceOrderLine("A-1", 2), new PlaceOrderLine("B-2", 1))))
            .Content.ReadFromJsonAsync<OrderResponse>();

        (await _client.DeleteAsync($"/orders/{placed!.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await StockAsync("A-1")).Should().Be(100);
        (await StockAsync("B-2")).Should().Be(5);
    }

    [Fact]
    public async Task ListOrders_25Orders_ReturnsTheSecondPage()
    {
        for (var i = 0; i < 25; i++)
            (await _client.PostAsJsonAsync("/orders", Request(new PlaceOrderLine("A-1", 1)))).EnsureSuccessStatusCode();

        var page = await _client.GetFromJsonAsync<Paginate<OrderSummary>>("/orders?index=1&size=10");

        page!.Items.Should().HaveCount(10);
        page.Count.Should().Be(25);
        page.Pages.Should().Be(3);
        page.Index.Should().Be(1);
    }
}

public sealed record RecordedEvent(Guid OrderId, bool CommittedWhenHandled);

public sealed class RecordedEvents
{
    private readonly ConcurrentQueue<RecordedEvent> _items = new();

    public IReadOnlyCollection<RecordedEvent> Items => _items;

    public void Add(RecordedEvent item) => _items.Enqueue(item);
}

/// <summary>Checks, from a separate scope and connection, that the order is already committed when the event arrives.</summary>
public sealed class RecordingOrderPlacedHandler(RecordedEvents recorded, IServiceScopeFactory scopes) : IDomainEventHandler<OrderPlaced>
{
    public async Task HandleAsync(OrderPlaced domainEvent, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var committed = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Orders
            .AnyAsync(o => o.Id == domainEvent.OrderId, cancellationToken);
        recorded.Add(new RecordedEvent(domainEvent.OrderId, committed));
    }
}
