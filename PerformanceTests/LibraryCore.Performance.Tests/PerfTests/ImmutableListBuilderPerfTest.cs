using BenchmarkDotNet.Attributes;
using LibraryCore.Performance.Tests.TestHarnessProvider;
using System.Collections.Immutable;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
public class ImmutableListBuilderPerfTest : IPerformanceTest
{
    public string CommandName => "ImmListBuilder";
    public string Description => "Run the immutable list builder vs List.ToImmutableList";

    [Params(5, 50, 100, 500, 1000, 10000)]
    public int NumberOfElements { get; set; }

    [Benchmark(Baseline = true)]
    public IImmutableList<int> ListToImmutableList()
    {
        var lst = new List<int>();

        foreach (var item in Enumerable.Range(0, NumberOfElements).ToList())
        {
            lst.Add(item);
        }

        return lst.ToImmutableList();
    }

    [Benchmark]
    public IImmutableList<int> RecordBaseBuilder()
    {
        var builder = ImmutableList.CreateBuilder<int>();

        foreach (var item in Enumerable.Range(0, NumberOfElements).ToList())
        {
            builder.Add(item);
        }

        return builder.ToImmutable();
    }
}
