using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Kck.Core.Abstractions.Entities;
using Kck.Core.Abstractions.Paging;
using Kck.Persistence.Abstractions.Dynamic;
using Kck.Persistence.Abstractions.Repositories;
using Kck.Persistence.Abstractions.UnitOfWork;
using Kck.Sample.Orders.Domain;
using Kck.Sample.Orders.Features.Customers;
using Kck.Sample.Orders.Features.Orders;
using Kck.Sample.Orders.Features.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kck.Sample.Orders.Tests;

/// <summary>End-to-end tests against SQLite; <see cref="PostgresOrdersTests"/> repeats the transactional ones.</summary>
#pragma warning disable CA1001 // IAsyncLifetime.DisposeAsync handles disposal
public sealed class OrdersTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly ApiFactory _factory = new();
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _client = _factory.CreateAuthenticatedClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => _factory.DisposeAsync().AsTask();

    [Fact]
    public async Task Orders_WithoutAuthentication_Return401()
    {
        var response = await _factory.CreateClient().GetAsync("/orders");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HealthReady_Returns200()
    {
        var response = await _factory.CreateClient().GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PlaceOrder_ValidRequest_Returns201AndPublishesConfirmationAfterCommit()
    {
        var response = await _client.PlaceOrderAsync(("A-1", 2));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order!.Total.Should().Be(20.00m);
        order.Status.Should().Be(nameof(OrderStatus.Placed));
        response.Headers.Location!.ToString().Should().Be($"/orders/{order.Id}");
        _factory.Services.GetRequiredService<RecordedConfirmations>().Items.Should().ContainSingle()
            .Which.Should().Be(new RecordedConfirmation(order.Id, CommittedWhenHandled: true));
        (await _factory.StockAsync("A-1")).Should().Be(98);
    }

    [Fact]
    public async Task PlaceOrder_InsufficientStock_Returns409AndPersistsNothing()
    {
        // B-2 has 5 in stock. The A-1 reservation made earlier in the same transaction is rolled back too.
        var response = await _client.PlaceOrderAsync(("A-1", 2), ("B-2", 6));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await _factory.QueryAsync(db => db.Orders.IgnoreQueryFilters().CountAsync())).Should().Be(0);
        _factory.Services.GetRequiredService<RecordedConfirmations>().Items.Should().BeEmpty();
        (await _factory.StockAsync("A-1")).Should().Be(100);
        (await _factory.StockAsync("B-2")).Should().Be(5);
    }

    [Fact]
    public async Task PayThenShip_MovesThroughTheStatuses()
    {
        var order = await _client.PlaceOrderAndReadAsync(("A-1", 1));

        var paid = await _client.PostAsync($"/orders/{order.Id}/pay", null);
        var shipped = await _client.PostAsync($"/orders/{order.Id}/ship", null);

        paid.StatusCode.Should().Be(HttpStatusCode.OK);
        (await paid.Content.ReadFromJsonAsync<OrderResponse>())!.Status.Should().Be(nameof(OrderStatus.Paid));
        shipped.StatusCode.Should().Be(HttpStatusCode.OK);
        (await shipped.Content.ReadFromJsonAsync<OrderResponse>())!.Status.Should().Be(nameof(OrderStatus.Shipped));
    }

    [Fact]
    public async Task ShipOrder_NotPaid_Returns409()
    {
        var order = await _client.PlaceOrderAndReadAsync(("A-1", 1));

        var response = await _client.PostAsync($"/orders/{order.Id}/ship", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await _client.GetFromJsonAsync<OrderResponse>($"/orders/{order.Id}"))!.Status.Should().Be(nameof(OrderStatus.Placed));
    }

    [Fact]
    public async Task CancelOrder_Shipped_Returns409AndKeepsTheOrder()
    {
        var order = await _client.PlaceOrderAndReadAsync(("A-1", 1));
        (await _client.PostAsync($"/orders/{order.Id}/pay", null)).EnsureSuccessStatusCode();
        (await _client.PostAsync($"/orders/{order.Id}/ship", null)).EnsureSuccessStatusCode();

        var response = await _client.DeleteAsync($"/orders/{order.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await _client.GetAsync($"/orders/{order.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CancelOrder_SoftDeletesOrderAndLines_KeepsCustomer()
    {
        var order = await _client.PlaceOrderAndReadAsync(("A-1", 1), ("B-2", 1));

        (await _client.DeleteAsync($"/orders/{order.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await _client.GetAsync($"/orders/{order.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var cancelled = await _client.GetFromJsonAsync<OrderResponse>($"/orders/{order.Id}?includeDeleted=true");
        cancelled!.IsDeleted.Should().BeTrue();
        cancelled.DeletedDate.Should().NotBeNull();
        cancelled.Lines.Should().HaveCount(2).And.OnlyContain(l => l.IsDeleted);
        (await _client.GetAsync($"/customers/{SampleData.DemoCustomerId}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCustomer_WithoutOrders_SoftDeletes()
    {
        var customer = await CreateCustomerAsync("Ada");

        (await _client.DeleteAsync($"/customers/{customer.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await _client.GetAsync($"/customers/{customer.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var deleted = await _client.GetFromJsonAsync<CustomerResponse>($"/customers/{customer.Id}?includeDeleted=true");
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteCustomer_WithCancelledOrder_Returns409PerDeleteBehavior()
    {
        // Order -> Customer is Restrict: even a cancelled (soft-deleted) order keeps the customer.
        var customer = await CreateCustomerAsync("Grace");
        var order = await _client.PlaceOrderAndReadAsync(customer.Id, ("A-1", 1));
        (await _client.DeleteAsync($"/orders/{order.Id}")).EnsureSuccessStatusCode();

        var response = await _client.DeleteAsync($"/customers/{customer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await _client.GetAsync($"/customers/{customer.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<CustomerResponse> CreateCustomerAsync(string name)
    {
        var created = await _client.PostAsJsonAsync("/customers", new CreateCustomerRequest(name));
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await created.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }

    [Fact]
    public async Task PlaceOrder_UnknownCustomer_Returns400()
    {
        var response = await _client.PlaceOrderAsync(Guid.CreateVersion7(), ("A-1", 1));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PlaceOrder_DispatcherFails_EventRetriedOnNextSave()
    {
        var flaky = new FlakyOrderPlacedHandler();
        await using var factory = new ApiFactory(configureServices: services =>
            services.AddSingleton<IDomainEventHandler<OrderPlaced>>(flaky));
        _ = factory.Server; // starts the host, which creates the schema

        await using var scope = factory.Services.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var order = Order.Place(SampleData.DemoCustomerId, [OrderLine.For(SampleData.Products[0], 1)]);
        await unitOfWork.BeginTransactionAsync();
        await unitOfWork.Repository<Order, Guid>().AddAsync(order);
        await unitOfWork.SaveChangesAsync();

        // The commit succeeds; the handler then fails, so the caller sees the error but the order is saved.
        var commit = () => unitOfWork.CommitAsync();
        await commit.Should().ThrowAsync<InvalidOperationException>().WithMessage("Simulated handler failure.");
        (await factory.QueryAsync(db => db.Orders.AnyAsync(o => o.Id == order.Id))).Should().BeTrue();

        // The event went back onto the order: the next save on the same unit of work dispatches it again.
        await unitOfWork.SaveChangesAsync();

        // At least once: OrderPlacedHandler ran before the failing handler, so the retry publishes the confirmation a
        // second time. This is why event handlers must be idempotent.
        flaky.Calls.Should().Be(2);
        factory.Services.GetRequiredService<RecordedConfirmations>().Items.Should().HaveCount(2)
            .And.OnlyContain(c => c.OrderId == order.Id && c.CommittedWhenHandled);
    }

    [Fact]
    public async Task ListOrders_25Orders_ReturnsTheSecondPage()
    {
        for (var i = 0; i < 25; i++)
            (await _client.PlaceOrderAsync(("A-1", 1))).EnsureSuccessStatusCode();

        var page = await _client.GetFromJsonAsync<Paginate<OrderSummary>>("/orders?index=1&size=10");

        page!.Items.Should().HaveCount(10);
        page.Count.Should().Be(25);
        page.Pages.Should().Be(3);
        page.Index.Should().Be(1);
    }

    [Fact]
    public async Task SearchOrders_WhitelistedField_ReturnsMatchingOrders()
    {
        var paid = await _client.PlaceOrderAndReadAsync(("A-1", 1));
        await _client.PlaceOrderAndReadAsync(("A-1", 1));
        (await _client.PostAsync($"/orders/{paid.Id}/pay", null)).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/orders/search",
            new DynamicQuery([new Sort("CreatedDate", "desc")], new Filter("Status", "eq", nameof(OrderStatus.Paid))));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<Paginate<OrderSummary>>();
        page!.Items.Should().ContainSingle().Which.Id.Should().Be(paid.Id);
    }

    [Fact]
    public async Task SearchOrders_GroupFilter_ReturnsMatchingOrders()
    {
        var paid = await _client.PlaceOrderAndReadAsync(("A-1", 1));
        await _client.PlaceOrderAndReadAsync(("A-1", 1));
        (await _client.PostAsync($"/orders/{paid.Id}/pay", null)).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/orders/search", new DynamicQuery([], new Filter("", "eq", null, "and",
        [
            new Filter("Status", "eq", nameof(OrderStatus.Paid)),
            new Filter("CustomerId", "eq", SampleData.DemoCustomerId),
        ])));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<Paginate<OrderSummary>>();
        page!.Items.Should().ContainSingle().Which.Id.Should().Be(paid.Id);
    }

    [Fact]
    public async Task SearchOrders_FieldOutsideWhitelist_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/orders/search",
            new DynamicQuery([], new Filter("IsDeleted", "eq", true)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("IsDeleted");
    }

    [Fact]
    public async Task ListProducts_SecondCall_IsServedFromTheCache()
    {
        var first = await _client.GetFromJsonAsync<List<CatalogueItem>>("/products");
        await _factory.QueryAsync(db => db.Products.Where(p => p.Id == "A-1")
            .ExecuteUpdateAsync(set => set.SetProperty(p => p.Name, "Renamed")));

        var second = await _client.GetFromJsonAsync<List<CatalogueItem>>("/products");

        first!.Should().Contain(p => p.Sku == "A-1" && p.Name == "Notebook");
        second.Should().BeEquivalentTo(first);
    }
}
