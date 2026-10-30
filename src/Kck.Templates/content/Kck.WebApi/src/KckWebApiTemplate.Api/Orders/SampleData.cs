namespace KckWebApiTemplate.Api.Orders;

/// <summary>Rows created with the schema so the API can be tried right away (see the .http file).</summary>
public static class SampleData
{
    public static readonly Guid DemoCustomerId = Guid.Parse("0199f0a0-0000-7000-8000-000000000001");

    private static readonly DateTime Seeded = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static Customer[] Customers =>
    [
        new() { Id = DemoCustomerId, Name = "Demo Customer", CreatedDate = Seeded },
    ];

    public static Product[] Products =>
    [
        new() { Id = "A-1", Name = "Notebook", UnitPrice = 10.00m, Stock = 100 },
        new() { Id = "B-2", Name = "Desk lamp", UnitPrice = 25.50m, Stock = 5 },
    ];
}
