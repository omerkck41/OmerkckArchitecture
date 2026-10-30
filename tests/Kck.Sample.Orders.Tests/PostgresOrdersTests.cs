using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Kck.Core.Abstractions.Paging;
using Kck.Persistence.Abstractions.Dynamic;
using Kck.Sample.Orders.Features.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Kck.Sample.Orders.Tests;

/// <summary>The transactional flows against a real PostgreSQL server (Docker required).</summary>
[Trait("Category", "Integration")]
#pragma warning disable CA1001 // IAsyncLifetime.DisposeAsync handles disposal
public sealed class PostgresOrdersTests : IAsyncLifetime
#pragma warning restore CA1001
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("kck_orders")
        .WithUsername("kck")
        .WithPassword("kck-test-password")
        .Build();

    private ApiFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _factory = new ApiFactory(_postgres.GetConnectionString());
        _client = _factory.CreateAuthenticatedClient();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task PlaceOrder_ValidRequest_CommitsAndPublishesConfirmation()
    {
        var response = await _client.PlaceOrderAsync(("A-1", 2));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        _factory.Services.GetRequiredService<RecordedConfirmations>().Items.Should().ContainSingle()
            .Which.Should().Be(new RecordedConfirmation(order!.Id, CommittedWhenHandled: true));
        (await _factory.StockAsync("A-1")).Should().Be(98);
    }

    [Fact]
    public async Task PlaceOrder_InsufficientStock_RollsBackTheReservation()
    {
        var response = await _client.PlaceOrderAsync(("A-1", 2), ("B-2", 6));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await _factory.QueryAsync(db => db.Orders.IgnoreQueryFilters().CountAsync())).Should().Be(0);
        _factory.Services.GetRequiredService<RecordedConfirmations>().Items.Should().BeEmpty();
        (await _factory.StockAsync("A-1")).Should().Be(100);
    }

    [Fact]
    public async Task SearchOrders_ByStatusAndDateWithoutOffset_Returns200()
    {
        // A date without an offset must reach Npgsql as UTC (timestamptz accepts only DateTimeKind.Utc).
        var paid = await _client.PlaceOrderAndReadAsync(("A-1", 1));
        (await _client.PostAsync($"/orders/{paid.Id}/pay", null)).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/orders/search", new DynamicQuery([], new Filter("", "eq", null, "and",
        [
            new Filter("Status", "eq", "Paid"),
            new Filter("CreatedDate", "gte", "2026-01-01"),
        ])));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<Paginate<OrderSummary>>();
        page!.Items.Should().ContainSingle().Which.Id.Should().Be(paid.Id);
    }

    [Fact]
    public async Task CancelOrder_SoftDeletesOrderAndLines()
    {
        var order = await _client.PlaceOrderAndReadAsync(("A-1", 1), ("B-2", 1));

        (await _client.DeleteAsync($"/orders/{order.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cancelled = await _client.GetFromJsonAsync<OrderResponse>($"/orders/{order.Id}?includeDeleted=true");
        cancelled!.IsDeleted.Should().BeTrue();
        cancelled.Lines.Should().HaveCount(2).And.OnlyContain(l => l.IsDeleted);
    }
}
