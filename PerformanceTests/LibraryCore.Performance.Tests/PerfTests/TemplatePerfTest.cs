using BenchmarkDotNet.Attributes;
using System.Text;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
[ReturnValueValidator(failOnError: true)]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.SlowestToFastest)]
public class TemplatePerfTest
{
    [GlobalSetup]
    public void Init()
    {
    }

    [Benchmark(Baseline = true)]
    public string CurrentImplementation()
    {
        return new StringBuilder()
           .Append($"Item {1}")
           .ToString();
    }

    [Benchmark]
    public string StringBuilderImplementation()
    {
        return new StringBuilder()
            .Append("Item 1")
            .ToString();
    }

}
