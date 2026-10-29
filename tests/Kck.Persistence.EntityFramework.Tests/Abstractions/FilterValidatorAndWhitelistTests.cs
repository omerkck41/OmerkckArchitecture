using System.Linq.Expressions;
using AwesomeAssertions;
using Kck.Persistence.Abstractions.Dynamic;
using Kck.Persistence.Abstractions.Security;
using Kck.Persistence.Abstractions.Specifications;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Abstractions;

public sealed class FilterValidatorAndWhitelistTests
{
    private sealed class Product
    {
        [Filterable] public string Name { get; set; } = string.Empty;
        [Filterable] public int Price { get; set; }
        public string Secret { get; set; } = string.Empty;
        public int Stock { get; set; }
    }

    private sealed class DefaultWhitelist : FilterPropertyWhitelist<Product>;

    private sealed class CustomWhitelist : FilterPropertyWhitelist<Product>
    {
        public CustomWhitelist()
        {
            Allow(p => p.Stock);
            Allow(p => p.Secret);
            Deny(p => p.Name);
        }
    }

    private sealed class BadWhitelist : FilterPropertyWhitelist<Product>
    {
        public BadWhitelist() => Allow(_ => 1);
    }

    // --- FilterValidator ---

    [Theory]
    [InlineData("eq")]
    [InlineData("NEQ")]
    [InlineData("lt")]
    [InlineData("Lte")]
    [InlineData("gt")]
    [InlineData("gte")]
    [InlineData("contains")]
    [InlineData("StartsWith")]
    [InlineData("endswith")]
    public void Validate_SupportedOperatorWithValue_ReturnsTrue(string op) =>
        FilterValidator.Validate(FilterFactory.F("F", op, "v")).Should().BeTrue();

    [Theory]
    [InlineData("isnull")]
    [InlineData("isnotnull")]
    public void Validate_NullOperatorWithoutValue_ReturnsTrue(string op) =>
        FilterValidator.Validate(FilterFactory.F("F", op, null)).Should().BeTrue();

    [Theory]
    [InlineData("isnull")]
    [InlineData("isnotnull")]
    public void Validate_NullOperatorWithValue_ReturnsFalse(string op) =>
        FilterValidator.Validate(FilterFactory.F("F", op, "x")).Should().BeFalse();

    [Fact]
    public void Validate_ValueOperatorWithoutValue_ReturnsFalse() =>
        FilterValidator.Validate(FilterFactory.F("F", "eq", null)).Should().BeFalse();

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_BlankField_ReturnsFalse(string field) =>
        FilterValidator.Validate(FilterFactory.F(field, "eq", 1)).Should().BeFalse();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("like")]
    public void Validate_BlankOrUnsupportedOperator_ReturnsFalse(string op) =>
        FilterValidator.Validate(FilterFactory.F("F", op, 1)).Should().BeFalse();

    // --- FilterPropertyWhitelist ---

    [Fact]
    public void Whitelist_Default_AllowsOnlyFilterableAttributedProperties()
    {
        var sut = new DefaultWhitelist();

        sut.AllowedProperties.Should().BeEquivalentTo("Name", "Price");
        sut.IsAllowed("Name").Should().BeTrue();
        sut.IsAllowed("Secret").Should().BeFalse();
    }

    [Fact]
    public void Whitelist_IsAllowed_IsCaseInsensitive()
    {
        var sut = new DefaultWhitelist();

        sut.IsAllowed("name").Should().BeTrue();
        sut.IsAllowed("PRICE").Should().BeTrue();
    }

    [Fact]
    public void Whitelist_AllowAndDeny_AdjustSetUsingValueAndReferenceMembers()
    {
        var sut = new CustomWhitelist();

        sut.IsAllowed("Stock").Should().BeTrue();   // value type => Convert-wrapped member
        sut.IsAllowed("Secret").Should().BeTrue();  // reference type => plain member
        sut.IsAllowed("Name").Should().BeFalse();   // denied after attribute scan
        sut.IsAllowed("Price").Should().BeTrue();
    }

    [Fact]
    public void Whitelist_NonMemberExpression_ThrowsArgumentException()
    {
        var act = () => new BadWhitelist();

        act.Should().Throw<ArgumentException>().WithMessage("*property access*");
    }

    // --- Specification ---

    private sealed class FullSpec : Specification<Product>
    {
        public FullSpec()
        {
            Where(p => p.Price > 5);
            AddInclude(p => p.Name);
            AddInclude("Category.Parent");
            ApplyOrderBy(p => p.Price);
            ApplyOrderByDescending(p => p.Stock);
            ApplyPaging(skip: 20, take: 10);
            IncludeDeleted();
            AsTracking();
        }
    }

    private sealed class EmptySpec : Specification<Product>;

    [Fact]
    public void Specification_Defaults_AreEmptyAndNoTrackingNoDeleted()
    {
        var spec = new EmptySpec();

        spec.Criteria.Should().BeNull();
        spec.Includes.Should().BeEmpty();
        spec.IncludeStrings.Should().BeEmpty();
        spec.OrderBy.Should().BeNull();
        spec.OrderByDescending.Should().BeNull();
        spec.Take.Should().BeNull();
        spec.Skip.Should().BeNull();
        spec.WithDeleted.Should().BeFalse();
        spec.EnableTracking.Should().BeFalse();
    }

    [Fact]
    public void Specification_ConfiguredInConstructor_ExposesAllSettings()
    {
        var spec = new FullSpec();

        spec.Criteria!.Compile()(new Product { Price = 6 }).Should().BeTrue();
        spec.Criteria.Compile()(new Product { Price = 5 }).Should().BeFalse();
        spec.Includes.Should().HaveCount(1);
        spec.IncludeStrings.Should().Equal("Category.Parent");
        spec.OrderBy.Should().NotBeNull();
        spec.OrderByDescending.Should().NotBeNull();
        spec.Skip.Should().Be(20);
        spec.Take.Should().Be(10);
        spec.WithDeleted.Should().BeTrue();
        spec.EnableTracking.Should().BeTrue();
    }

    [Fact]
    public void Specification_Collections_AreReadOnlyViews()
    {
        var spec = new FullSpec();

        ((ICollection<Expression<Func<Product, object>>>)spec.Includes).IsReadOnly.Should().BeTrue();
        ((ICollection<string>)spec.IncludeStrings).IsReadOnly.Should().BeTrue();
    }
}


