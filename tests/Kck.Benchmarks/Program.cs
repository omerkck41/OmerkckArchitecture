using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Running;

namespace Kck.Benchmarks;

internal static class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance.AddExporter(JsonExporter.Brief);
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}
