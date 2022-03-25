using BenchmarkDotNet.Attributes;
using LibraryCore.Performance.Tests.TestHarnessProvider;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
public class StringReaderFuncVsDoublePeekPerfTest : IPerformanceTest
{
    public string CommandName => "StringReaderFuncVs2Peek";
    public string Description => "Run performance test on invoking a func vs calling peek twice";

    [Benchmark(Baseline = true)]
    public int DoublePeek()
    {
        using var reader = new StringReader("abcdef");
        int c = 0;

        while (reader.Peek() != -1 && (char)reader.Peek() != 'z')
        {
            c = reader.Read();
        }

        return c;
    }

    [Benchmark]
    public int FuncInvoke()
    {
        using var reader = new StringReader("abcdef");
        int c = 0;

        while (Helper(reader, x => (char)x != 'z'))
        {
            c = reader.Read();
        }

        return c;
    }

    private static bool Helper(StringReader reader, Func<int, bool> tester)
    {
        var peek = reader.Peek();

        return peek != -1 && tester(peek);
    }

}