using AwesomeAssertions;
using Kck.Persistence.Abstractions.Dynamic;
using Xunit;

namespace Kck.Persistence.EntityFramework.Tests.Dynamic;

public sealed class FilterConstructionTests
{
    [Fact]
    public void Filter_ParameterizedConstructors_SetTheRequiredMembers()
    {
        var byString = new Filter("Name", "eq", "a");
        var byEnum = new Filter("Name", FilterOperator.Contains, "a");

        byString.Field.Should().Be("Name");
        byString.Operator.Should().Be("eq");
        byEnum.Operator.Should().Be("contains");
    }

    [Fact]
    public void Sort_ParameterizedConstructor_SetsTheRequiredMembers()
    {
        var sort = new Sort("Name", "desc", priority: 1);

        sort.Field.Should().Be("Name");
        sort.Dir.Should().Be("desc");
        sort.Priority.Should().Be(1);
    }
}
