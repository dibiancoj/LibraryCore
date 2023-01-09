using BenchmarkDotNet.Attributes;
using LibraryCore.Core.Delimiter;
using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Readers;
using System.Text;
using static LibraryCore.Core.Delimiter.DelimiterBuilder;
using static LibraryCore.Performance.Tests.Program;

namespace LibraryCore.Performance.Tests.PerfTests.Tests;

[SimpleJob]
[Config(typeof(Config))]
[MemoryDiagnoser]
public class DelimiterReaderPerfTest
{
    [Params(@"""abc""|""cde""|""fgh""", @"""abc""|""cde""|""fgh"",""zz""|""bb""|""gg""", @"""abc""|""cde""|""fgh"",""zz""|""bb""|""gg""|""abc""|""cde""|""fgh"",""zz""|""bb""|""gg""|""abc""|""cde""|""fgh"",""zz""|""bb""|""gg""")]
    public string TextToParse { get; set; }

    [Benchmark(Baseline = true)]
    public List<DelimiterRow> WithStringReader()
    {
        return DelimiterReader.ParseFromTextLinesLazy(TextToParse, '|').ToList();
    }

    [Benchmark]
    public List<DelimiterRow> WithStructSpanReader()
    {
        return WithStructSpan.ParseFromTextLinesLazyNew(TextToParse, '|').ToList();
    }
}

public static class WithStructSpan
{
    public static IEnumerable<DelimiterRow> ParseFromTextLinesLazyNew(string contentToParse, char delimiter)
    {
        //i profiled this and its faster and more memory efficient to use a string reader for each row. To do this for every column had to allocate to much.
        //the current implementation is best for reducing memory
        using var reader = new StringReader(contentToParse);

        //loop until we are done
        while (reader.HasMoreCharacters())
        {
            var lineToRead = reader.ReadLine();

            if (lineToRead.HasValue())
            {
                //grab the line to parse and split the raw data into specific columns
                yield return new DelimiterRow(ParseLineIntoColumns(lineToRead, delimiter));
            }
        }
    }

    private static IList<string> ParseLineIntoColumns(string lineToRead, char delimiter)
    {
        const char quoteCharacter = '"';
        var columnsParsed = new List<string>();
        string workingColumnParsed = null;

        var reader = new StringSpanReader(lineToRead);

        //"field 1","field 2","canbe""3"
        //, "field2", "field 3"
        //"1","2",,
        //field 1, field 2
        //"field 1", field2
        while (reader.HasMoreCharacters())
        {
            //this method should loop through the characters and each the specific columns.
            //the delimiter will be eaten here.

            var currentCharacter = reader.ReadCharacter();

            if (currentCharacter == quoteCharacter)
            {
                //walk word. This will eat everything from quote to quote - including the quote
                workingColumnParsed = WalkColumnWord(ref reader, quoteCharacter);
            }
            else if (currentCharacter == delimiter)
            {
                //push the working column into the list
                columnsParsed.Add(workingColumnParsed);
                workingColumnParsed = null;
            }
            else
            {
                //normal word without quotes...walk it and return the entire column. The delimiter after the work will be left
                workingColumnParsed = WalkColumnWordWithoutQuotes(ref reader, currentCharacter, delimiter);
            }
        }

        //add the last column to the list
        columnsParsed.Add(workingColumnParsed);

        return columnsParsed;
    }

    private static string WalkColumnWordWithoutQuotes(ref StringSpanReader reader, char currentCharacterRead, char delimiter)
    {
        return string.Concat(currentCharacterRead, reader.ReadUntilCharacter(new string(new[] { delimiter }), StringComparison.OrdinalIgnoreCase));
        //var columnBuilder = new StringBuilder().Append(currentCharacterRead);

        //while (reader.PeekCharacter() != delimiter && reader.HasMoreCharacters())
        //{
        //    columnBuilder.Append(reader.ReadCharacter());
        //}

        //return columnBuilder.ToString();
    }

    private static string WalkColumnWord(ref StringSpanReader reader, char quoteCharacter)
    {
        var columnBuilder = new StringBuilder();

        while (reader.HasMoreCharacters())
        {
            var currentCharacter = reader.ReadCharacter();
            var peakedCharacter = reader.PeekCharacter();

            //is if a quote is in a column it would be double quoted to escape it.

            //so if the next character is not a quote...then break because the word is complete
            if (currentCharacter == quoteCharacter && peakedCharacter != quoteCharacter)
            {
                break;
            }
            //if we have double quotes...the eat one of them and record the next
            else if (currentCharacter == quoteCharacter && peakedCharacter == quoteCharacter)
            {
                //eat 1 of the quotes
                reader.ReadCharacter();
            }

            columnBuilder.Append(currentCharacter);
        }

        return columnBuilder.ToString();
    }
}
