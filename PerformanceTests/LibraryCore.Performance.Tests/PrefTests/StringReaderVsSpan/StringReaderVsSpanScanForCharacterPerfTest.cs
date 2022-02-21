using BenchmarkDotNet.Attributes;
using LibraryCore.Core.Readers;
using LibraryCore.Performance.Tests.TestHarnessProvider;
using System.Text;

namespace LibraryCore.Performance.Tests.PrefTests;

[SimpleJob]
[MemoryDiagnoser]
public class StringReaderVsSpanScanForCharacterPerfTest : IPerformanceTest
{
    public string CommandName => "StringReaderVsSpan.ScanForCharacter";
    public string Description => "Scan for a character in a string reader vs string span reader";

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