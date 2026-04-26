using BenchmarkDotNet.Attributes;
using Kck.Core.Abstractions.Results;

namespace Kck.Benchmarks;

[MemoryDiagnoser]
public class ResultBenchmarks
{
    private static readonly Error CachedError = new("BENCH", "benchmark error");

    [Benchmark]
    public Result<int> Success_Factory() => Result<int>.Success(42);

    [Benchmark]
    public Result<int> Failure_Factory() => Result<int>.Failure(CachedError);

    [Benchmark]
    public string Match_Success()
    {
        var r = Result<int>.Success(42);
        return r.Match(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture), e => e.Message);
    }

    [Benchmark]
    public string Match_Failure()
    {
        var r = Result<int>.Failure(CachedError);
        return r.Match(v => v.ToString(System.Globalization.CultureInfo.InvariantCulture), e => e.Message);
    }
}
