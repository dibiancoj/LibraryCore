using BenchmarkDotNet.Attributes;
using LibraryCore.Core.Readers;
using System.Text;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests.StringReaderVsSpan;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
[ReturnValueValidator(failOnError: true)]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.SlowestToFastest)]
public class StringReaderVsSpanReadPerfTest
{
    [Params("1 == 1",
            "'one' =asdf asdf asdffasd fasdf asdasdf asdfasasdfasddfasdfasd= 'one'",
            "1 == 1 && 2 == 2 && true == true asdfasdfasd assd fasd fasdf asdf asdf asdf asdf asdf asdfasdfasddf asdf asdf asdf == true asdfasdfasd assd fasd fasdf asdf asdf asdf asdf asdf asdfasdfasddf asdf asdf asdf")]
    public string CodeToParse { get; set; }

    [Benchmark(Baseline = true)]
    public string StringReader()
    {
        var sb = new StringBuilder();
        using var reader = new StringReader(CodeToParse);

        while (reader.Peek() != -1)
        {
            reader.Peek();
            sb.Append((char)reader.Read());
        }

        return sb.ToString();
    }

    [Benchmark]
    public string RawStringReader()
    {
        var sb = new StringBuilder();
        var reader = new RawStringReader(CodeToParse);

        while (reader.Peek() != -1)
        {
            reader.Peek();
            sb.Append((char)reader.Read());
        }

        return sb.ToString();
    }

    [Benchmark]
    public string ClassSpanStringReader()
    {
        var sb = new StringBuilder();
        var reader = new ClassSpanStringReader(CodeToParse);

        while (reader.Peek() != -1)
        {
            reader.Peek();
            sb.Append(reader.Read());
        }

        return sb.ToString();
    }

    [Benchmark]
    public string StructSpanStringReader()
    {
        var sb = new StringBuilder();
        var reader = new StructSpanStringReader(CodeToParse);

        while (reader.Peek() != -1)
        {
            reader.Peek();
            sb.Append((char)reader.Read());
        }

        return sb.ToString();
    }

    [Benchmark]
    public string StructSpanStringReaderInLibrary()
    {
        var sb = new StringBuilder();
        var reader = new StringSpanReader(CodeToParse);

        while (reader.HasMoreCharacters())
        {
            reader.PeekCharacter();
            sb.Append(reader.ReadCharacter());
        }

        return sb.ToString();
    }
}

public class RawStringReader
{
    public RawStringReader(string stringToRead)
    {
        StringToRead = stringToRead;
    }

    private string StringToRead { get; }
    private int Index { get; set; }

    public int Peek() => StringToRead.Length == Index ? -1 : StringToRead[Index];

    public int Read()
    {
        var text = StringToRead[Index];

        Index++;

        return text;
    }
}

public class ClassSpanStringReader
{
    public ClassSpanStringReader(string stringToRead)
    {
        StringToRead = stringToRead;
    }

    private string StringToRead { get; }
    private int Index { get; set; }

    public int Peek() => StringToRead.Length == Index ? -1 : StringToRead.AsSpan()[Index];

    public char Read()
    {
        var text = StringToRead.AsSpan()[Index];

        Index++;

        return text;
    }
}

public ref struct StructSpanStringReader
{
    public StructSpanStringReader(string stringToRead)
    {
        StringToRead = stringToRead.AsSpan();
        Index = 0;
    }

    private ReadOnlySpan<char> StringToRead { get; }
    private int Index { get; set; }

    public int Peek() => StringToRead.Length == Index ? -1 : StringToRead[Index];

    public int Read()
    {
        return StringToRead[Index++];
    }
}
