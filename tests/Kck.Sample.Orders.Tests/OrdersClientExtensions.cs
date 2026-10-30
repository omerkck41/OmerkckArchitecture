using System.Net.Http.Json;
using Kck.Sample.Orders.Domain;
using Kck.Sample.Orders.Features.Orders;

namespace Kck.Sample.Orders.Tests;

internal static class OrdersClientExtensions
{
    public static Task<HttpResponseMessage> PlaceOrderAsync(this HttpClient client, params (string Sku, int Quantity)[] lines) =>
        client.PlaceOrderAsync(SampleData.DemoCustomerId, lines);

    public static Task<HttpResponseMessage> PlaceOrderAsync(
        this HttpClient client, Guid customerId, params (string Sku, int Quantity)[] lines) =>
        client.PostAsJsonAsync("/orders", new PlaceOrderRequest(customerId, [.. lines.Select(l => new PlaceOrderLine(l.Sku, l.Quantity))]));

    public static Task<OrderResponse> PlaceOrderAndReadAsync(this HttpClient client, params (string Sku, int Quantity)[] lines) =>
        client.PlaceOrderAndReadAsync(SampleData.DemoCustomerId, lines);

    public static async Task<OrderResponse> PlaceOrderAndReadAsync(
        this HttpClient client, Guid customerId, params (string Sku, int Quantity)[] lines)
    {
        var response = await client.PlaceOrderAsync(customerId, lines);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
    }
}
