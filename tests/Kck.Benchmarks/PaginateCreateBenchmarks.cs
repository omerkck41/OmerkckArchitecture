using BenchmarkDotNet.Attributes;
using Kck.Core.Abstractions.Paging;

namespace Kck.Benchmarks;

[MemoryDiagnoser]
public class PaginateCreateBenchmarks
{
    private readonly IReadOnlyList<int> _items = Enumerable.Range(0, 50).ToArray();

    [Params(10, 100, 1000)]
    public int Size { get; set; }

    [Params(0, 5)]
    public int Index { get; set; }

    [Benchmark]
    public Paginate<int> Create() =>
        Paginate<int>.Create(_items, totalCount: 10_000, index: Index, size: Size);
}
