using BenchmarkDotNet.Attributes;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests.Tests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
[ReturnValueValidator(failOnError: true)]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.SlowestToFastest)]
public class StringReaderFuncVsDoublePeekPerfTest
{
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