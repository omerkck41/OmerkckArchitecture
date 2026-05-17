using FluentAssertions;
using Kck.Search.Elasticsearch.DependencyInjection;
using Xunit;

namespace Kck.Search.Elasticsearch.Tests;

public sealed class ElasticsearchOptionsValidatorTests
{
    private readonly ElasticsearchOptionsValidator _sut = new();

    [Fact]
    public void Validate_ValidOptions_ReturnsSuccess()
    {
        var opts = new ElasticsearchOptions
        {
            ConnectionString = "https://localhost:9200",
            NumberOfShards = 1,
            NumberOfReplicas = 0
        };
        _sut.Validate(null, opts).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyConnectionString_ReturnsFail(string cs)
    {
        var opts = new ElasticsearchOptions { ConnectionString = cs };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ConnectionString");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://localhost:9200")]
    public void Validate_InvalidUri_ReturnsFail(string cs)
    {
        var opts = new ElasticsearchOptions { ConnectionString = cs };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("ConnectionString");
    }

    [Fact]
    public void Validate_ZeroShards_ReturnsFail()
    {
        var opts = new ElasticsearchOptions { ConnectionString = "https://localhost:9200", NumberOfShards = 0 };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("NumberOfShards");
    }

    [Fact]
    public void Validate_NegativeReplicas_ReturnsFail()
    {
        var opts = new ElasticsearchOptions { ConnectionString = "https://localhost:9200", NumberOfReplicas = -1 };
        var result = _sut.Validate(null, opts);
        result.Succeeded.Should().BeFalse();
        result.FailureMessage.Should().Contain("NumberOfReplicas");
    }
}
