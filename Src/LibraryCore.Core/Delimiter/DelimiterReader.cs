using LibraryCore.Core.ExtensionMethods;
using System.Text;
using static LibraryCore.Core.Delimiter.DelimiterBuilder;

namespace LibraryCore.Core.Delimiter;

public static class DelimiterReader
{
    public static IEnumerable<DelimiterRow> ParseFromLinesLazy(IEnumerable<string> linesOfContent, char delimiter) => linesOfContent.Select(x => new DelimiterRow(ParseLineIntoColumns(x, delimiter)));

    /// <summary>
    /// Parse The File From A Text String
    /// </summary>
    /// <param name="contentToParse">All the lines of content that we are want to parse</param>
    /// <param name="delimiter">Delimiter That Each Column Is Seperated By</param>
    /// <returns>IEnumerable ParseRowResult. Holds each of the rows. Inside that object holds the columns for that row</returns> 
    /// <remarks>Method is lazy loaded. File will be locked until method is complete. Call ToArray() To Push To List</remarks>
    public static IEnumerable<DelimiterRow> ParseFromTextLinesLazy(string contentToParse, char delimiter)
    {
        //i profiled this and its faster and more memory efficient to use a string reader for each row. To do this for every column had to allocate to much.
        //the current implementation is best for reducing memory
        using var reader = new StringReader(contentToParse);

        //loop until we are done
        while (reader.Peek() != -1)
        {
            var lineToRead = reader.ReadLine();

            if (lineToRead.HasValue())
            {
                //grab the line to parse and split the raw data into specific columns
                yield return new DelimiterRow(ParseLineIntoColumns(lineToRead, delimiter));
            }
        }
    }

    private static IList<string?> ParseLineIntoColumns(string lineToRead, char delimiter)
    {
        var columnsParsed = new List<string?>();
        const char quoteCharacter = '"';

        using var reader = new StringReader(lineToRead);

        //"field 1","field 2","canbe""3"
        //, "field2", "field 3"
        //"1","2",,
        while (reader.HasMoreCharacters())
        {
            var currentCharacter = reader.ReadCharacter();

            if (currentCharacter == quoteCharacter)
            {
                //walk word
                columnsParsed.Add(WalkColumnWord(reader, quoteCharacter));
            }
            else if (currentCharacter == delimiter && !reader.HasMoreCharacters())
            {
                //blank word at the end of the file
                columnsParsed.Add(string.Empty);
                columnsParsed.Add(string.Empty);
            }
            else if (currentCharacter == delimiter)
            {
                columnsParsed.Add(string.Empty);
            }
        }

        return columnsParsed;
    }

    private static string WalkColumnWord(StringReader reader, char quoteCharacter)
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
                reader.Read();
            }

            columnBuilder.Append(currentCharacter);
        }

        //eat the comma
        if (reader.HasMoreCharacters())
        {
            reader.Read();
        }

        return columnBuilder.ToString();
    }

}
