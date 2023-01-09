using BenchmarkDotNet.Attributes;
using LibraryCore.Core.Readers;
using System.Text;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests.StringReaderVsSpan;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
public class StringReaderVsSpanScanForCharacterPerfTest
{
    [Params("1 == 1 && 2 == 2",
            "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz && zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz",
            "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz && zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public string CodeToParse { get; set; }

    [Benchmark(Baseline = true)]
    public string StringReader()
    {
        var sb = new StringBuilder();
        using var reader = new StringReader(CodeToParse);

        while (reader.Peek() != -1 && reader.Peek() != '&')
        {
            sb.Append((char)reader.Read());
        }

        return sb.ToString();
    }

    [Benchmark]
    public string StructSpanStringReaderInLibrary()
    {
        var reader = new StringSpanReader(CodeToParse);

        return reader.ReadUntilCharacter("&", StringComparison.OrdinalIgnoreCase);
    }
}