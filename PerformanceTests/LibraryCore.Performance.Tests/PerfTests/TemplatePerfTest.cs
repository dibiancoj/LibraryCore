using BenchmarkDotNet.Attributes;
using System.Text;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
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
