using AwesomeAssertions;
using Kck.Persistence.Abstractions.Dynamic;
using Kck.Persistence.EntityFramework.Security;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Security;

public class DynamicFilterWhitelistGuardTests
{
    private static readonly TestWhitelist<TestEntity> Whitelist = new("Name", "Value");

    [Fact]
    public void Validate_NullQuery_Throws()
    {
        var act = () => DynamicFilterWhitelistGuard.Validate(null!, Whitelist);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_NullWhitelist_Throws()
    {
        var query = new DynamicQuery();
        var act = () => DynamicFilterWhitelistGuard.Validate<TestEntity>(query, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_AllFieldsAllowed_DoesNotThrow()
    {
        var query = new DynamicQuery
        {
            Filter = new Filter
            {
                Field = "Name",
                Operator = "eq",
                Value = "x",
                Filters = [new Filter { Field = "Value", Operator = "gt", Value = 1 }],
            },
            Sort = [new Sort { Field = "Name", Dir = "asc" }],
        };

        var act = () => DynamicFilterWhitelistGuard.Validate(query, Whitelist);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_TopLevelFilterFieldNotAllowed_Throws()
    {
        var query = new DynamicQuery
        {
            Filter = new Filter { Field = "Secret", Operator = "eq", Value = "x" },
        };

        var act = () => DynamicFilterWhitelistGuard.Validate(query, Whitelist);

        act.Should().Throw<ArgumentException>().WithMessage("*Secret*");
    }

    [Fact]
    public void Validate_NestedFilterFieldNotAllowed_Throws()
    {
        // Nested disallowed field — kills the recursion / Filters-null-check mutants.
        var query = new DynamicQuery
        {
            Filter = new Filter
            {
                Field = "Name",
                Operator = "eq",
                Value = "x",
                Filters = [new Filter { Field = "Secret", Operator = "eq", Value = "y" }],
            },
        };

        var act = () => DynamicFilterWhitelistGuard.Validate(query, Whitelist);

        act.Should().Throw<ArgumentException>().WithMessage("*Secret*");
    }

    [Fact]
    public void Validate_SortFieldNotAllowed_Throws()
    {
        // Null filter, disallowed sort — kills the sort-loop and Filter-null branch.
        var query = new DynamicQuery
        {
            Filter = null,
            Sort = [new Sort { Field = "Secret", Dir = "asc" }],
        };

        var act = () => DynamicFilterWhitelistGuard.Validate(query, Whitelist);

        act.Should().Throw<ArgumentException>().WithMessage("*Secret*");
    }
}
