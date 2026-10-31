using System.Text.Json;
using AwesomeAssertions;
using Kck.Persistence.Abstractions.Dynamic;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Abstractions;

/// <summary>
/// Filters as an API receives them: deserialized from JSON, so <see cref="Filter.Value"/> is a <see cref="JsonElement"/>
/// (or a string from a query string), not a value of the property's type.
/// </summary>
#pragma warning disable IL2026, IL3050
public sealed class DynamicQueryJsonValueTests
{
    private enum Status
    {
        Placed,
        Paid,
    }

    private sealed class Row
    {
        public string Name { get; init; } = string.Empty;
        public Status Status { get; init; }
        public Guid Id { get; init; }
        public int Quantity { get; init; }
        public decimal Total { get; init; }
        public bool Active { get; init; }
        public DateTime Created { get; init; }
        public int? Rank { get; init; }
    }

    private static readonly Guid SecondId = Guid.Parse("0199f0a0-0000-7000-8000-000000000002");

    private static readonly Row[] Data =
    [
        new() { Name = "a", Status = Status.Placed, Id = Guid.Parse("0199f0a0-0000-7000-8000-000000000001"), Quantity = 1, Total = 10.5m, Active = true, Created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), Rank = null },
        new() { Name = "b", Status = Status.Paid, Id = SecondId, Quantity = 5, Total = 99.99m, Active = false, Created = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), Rank = 2 },
    ];

    private static string[] Run(string filterJson)
    {
        var query = JsonSerializer.Deserialize<DynamicQuery>($$"""{"filter":{{filterJson}}}""", JsonSerializerOptions.Web)!;
        return Data.AsQueryable().ToDynamic(query).Select(r => r.Name).ToArray();
    }

    [Fact]
    public void ToDynamic_JsonStringOnEnum_MatchesByName() =>
        Run("""{"field":"Status","operator":"eq","value":"Paid"}""").Should().Equal("b");

    [Fact]
    public void ToDynamic_JsonStringOnEnum_IgnoresCase() =>
        Run("""{"field":"Status","operator":"eq","value":"paid"}""").Should().Equal("b");

    [Fact]
    public void ToDynamic_JsonNumberOnEnum_MatchesByValue() =>
        Run("""{"field":"Status","operator":"eq","value":1}""").Should().Equal("b");

    [Fact]
    public void ToDynamic_JsonStringOnGuid_Matches() =>
        Run($$"""{"field":"Id","operator":"eq","value":"{{SecondId}}"}""").Should().Equal("b");

    [Fact]
    public void ToDynamic_JsonNumberOnInt_Compares() =>
        Run("""{"field":"Quantity","operator":"gt","value":2}""").Should().Equal("b");

    [Fact]
    public void ToDynamic_JsonNumberOnDecimal_Compares() =>
        Run("""{"field":"Total","operator":"lt","value":50.25}""").Should().Equal("a");

    [Fact]
    public void ToDynamic_JsonBoolean_Matches() =>
        Run("""{"field":"Active","operator":"eq","value":true}""").Should().Equal("a");

    [Fact]
    public void ToDynamic_JsonStringOnDateTime_Compares() =>
        Run("""{"field":"Created","operator":"gte","value":"2026-03-01T00:00:00Z"}""").Should().Equal("b");

    [Fact]
    public void ToDynamic_JsonNumberOnNullableInt_Matches() =>
        Run("""{"field":"Rank","operator":"eq","value":2}""").Should().Equal("b");

    [Fact]
    public void ToDynamic_JsonNullOnNullableInt_MatchesNull() =>
        Run("""{"field":"Rank","operator":"eq","value":null}""").Should().Equal("a");

    [Fact]
    public void ToDynamic_JsonString_StillWorksOnString() =>
        Run("""{"field":"Name","operator":"contains","value":"b"}""").Should().Equal("b");

    [Fact]
    public void DynamicQuery_SortNullInJson_IsEmpty()
    {
        // An API client may send "sort": null; the guard and ToDynamic iterate Sort.
        var query = JsonSerializer.Deserialize<DynamicQuery>("""{"filter":null,"sort":null}""", JsonSerializerOptions.Web)!;

        query.Sort.Should().BeEmpty();
        Data.AsQueryable().ToDynamic(query).Should().HaveCount(2);
    }

    [Fact]
    public void ToDynamic_StringOnInt_IsParsed()
    {
        var query = new DynamicQuery([], new Filter("Quantity", FilterOperator.Eq, "5"));

        Data.AsQueryable().ToDynamic(query).Select(r => r.Name).Should().Equal("b");
    }

    [Fact]
    public void ToDynamic_ValueOfOtherNumericType_IsConverted()
    {
        var query = new DynamicQuery([], new Filter("Quantity", FilterOperator.Eq, 5L));

        Data.AsQueryable().ToDynamic(query).Select(r => r.Name).Should().Equal("b");
    }

    [Theory]
    [InlineData("2026-03-01")]
    [InlineData("2026-03-01T03:00:00+03:00")]
    [InlineData("2026-03-01T00:00:00Z")]
    public void ToDynamic_DateWithoutOrWithOffset_IsComparedAsUtc(string value)
    {
        // Npgsql only accepts DateTimeKind.Utc for timestamptz columns; audit dates are UTC.
        var query = JsonSerializer.Deserialize<DynamicQuery>(
            $$$"""{"filter":{"field":"Created","operator":"gte","value":"{{{value}}}"}}""", JsonSerializerOptions.Web)!;

        var constant = new ConstantFinder<DateTime>().Find(Data.AsQueryable().ToDynamic(query).Expression);

        constant.Should().Be(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        constant.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ToDynamic_OrderingOperatorOnEnum_ThrowsArgumentException()
    {
        var act = () => Run("""{"field":"Status","operator":"gt","value":"Placed"}""");

        act.Should().Throw<ArgumentException>().WithMessage("*'gt'*'Status'*");
    }

    private sealed class ConstantFinder<TValue> : System.Linq.Expressions.ExpressionVisitor
    {
        private TValue? _found;

        public TValue Find(System.Linq.Expressions.Expression expression)
        {
            Visit(expression);
            return _found ?? throw new InvalidOperationException($"No {typeof(TValue).Name} constant in the expression.");
        }

        protected override System.Linq.Expressions.Expression VisitConstant(System.Linq.Expressions.ConstantExpression node)
        {
            if (node.Value is TValue value)
                _found = value;
            return base.VisitConstant(node);
        }
    }

    [Theory]
    [InlineData("""{"field":"Status","operator":"eq","value":"Shipped"}""", "Status")]
    [InlineData("""{"field":"Id","operator":"eq","value":"not-a-guid"}""", "Id")]
    [InlineData("""{"field":"Quantity","operator":"eq","value":"many"}""", "Quantity")]
    [InlineData("""{"field":"Quantity","operator":"eq","value":null}""", "Quantity")]
    [InlineData("""{"field":"Active","operator":"eq","value":{"x":1}}""", "Active")]
    public void ToDynamic_ValueNotValidForTheProperty_ThrowsArgumentException(string filterJson, string field)
    {
        var act = () => Run(filterJson);

        act.Should().Throw<ArgumentException>().WithMessage($"*'{field}'*");
    }
}
