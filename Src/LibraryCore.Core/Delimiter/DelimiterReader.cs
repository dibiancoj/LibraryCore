using LibraryCore.Core.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static LibraryCore.Core.Delimiter.DelimiterBuilder;

namespace LibraryCore.Core.Delimiter;

public static class DelimiterReader
{

    public static IEnumerable<DelimiterRow> ParseFromLinesLazy(IEnumerable<string> linesOfContent, string delimiter) => linesOfContent.Select(x => ParseLine(x, delimiter));

    /// <summary>
    /// Parse The File From A Text String
    /// </summary>
    /// <param name="contentToParse">All the lines of content that we are want to parse</param>
    /// <param name="delimiter">Delimiter That Each Column Is Seperated By</param>
    /// <returns>IEnumerable ParseRowResult. Holds each of the rows. Inside that object holds the columns for that row</returns> 
    /// <remarks>Method is lazy loaded. File will be locked until method is complete. Call ToArray() To Push To List</remarks>
    public static IEnumerable<DelimiterRow> ParseFromTextLinesLazy(string contentToParse, string delimiter)
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
                yield return ParseLine(lineToRead, delimiter);
            }
        }
    }

    private static DelimiterRow ParseLine(string lineToRead, string delimiter) => new(lineToRead.Split(delimiter, StringSplitOptions.None));

}
