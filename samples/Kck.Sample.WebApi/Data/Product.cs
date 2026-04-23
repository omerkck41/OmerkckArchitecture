using Kck.Core.Abstractions.Entities;

namespace Kck.Sample.WebApi.Data;

public sealed class Product : Entity<Guid>
{
    public string Name { get; set; } = default!;
    public string Category { get; set; } = default!;
    public decimal Price { get; set; }
}
