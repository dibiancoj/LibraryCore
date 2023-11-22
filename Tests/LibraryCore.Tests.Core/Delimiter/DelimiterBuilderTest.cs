using LibraryCore.Core.Delimiter;
using static LibraryCore.Core.Delimiter.DelimiterBuilder;

namespace LibraryCore.Tests.Core.Delimiter;

public class DelimiterBuilderTest
{
    #region Delimiter Creator Framework

    /// <summary>
    /// CSV Delimiter
    /// </summary>
    private const char CsvDelimiter = ',';

    /// <summary>
    /// Split the output text
    /// </summary>
    /// <param name="outputValue">Value to output</param>
    /// <returns>column data</returns>
    private static IEnumerable<DelimiterRow> ParseResultsLazy(string outputValue)
    {
        return DelimiterReader.ParseFromTextLinesLazy(outputValue, CsvDelimiter);
    }

    #endregion

    #region Without Headers With A Single Line

    [Fact]
    public void CSVWithoutHeadersTest1()
    {
        //create the delimiter builder
        var delimiterBuilder = new DelimiterBuilder(CsvDelimiter);

        //add a row of data
        delimiterBuilder.AddRow(new string?[] { "1", "2", "", null });

        //add a row to test the none row
        delimiterBuilder.AddRow(Array.Empty<string>());

        //what is the final output of creator
        var result = delimiterBuilder.WriteData();

        //split the text result into columns
        var splitTextResult = ParseResultsLazy(result).ToArray();

        //check the results
        Assert.Equal(1, splitTextResult[0].Value<int?>(0));
        Assert.Equal(2, splitTextResult[0].Value<int?>(1));
        Assert.Null(splitTextResult[0].Value<int?>(2));
        Assert.Null(splitTextResult[0].Value<int?>(3));
    }

    [Fact]
    public void CSVWithoutHeadersTest2()
    {
        //create the delimiter builder
        var delimiterBuilder = new DelimiterBuilder(CsvDelimiter);

        //add a row
        delimiterBuilder.AddRow(new string?[] { "1", "2", "3", "4" });

        //what is the final output of creator
        var result = delimiterBuilder.WriteData();

        //split the text result into columns
        var splitTextResult = ParseResultsLazy(result).ToArray();

        //check the results
        Assert.Equal(1, splitTextResult[0].Value<int?>(0));
        Assert.Equal(2, splitTextResult[0].Value<int?>(1));
        Assert.Equal(3, splitTextResult[0].Value<int?>(2));
        Assert.Equal(4, splitTextResult[0].Value<int?>(3));
    }

    #endregion

    #region Without Headers With A Single Line (Bulk Load)

    [Fact]
    public void CSVWithoutHeadersBulkLoadTest1()
    {
        //create the delimiter builder
        var delimiterBuilder = new DelimiterBuilder(CsvDelimiter);

        //create list of rows
        var rowsToAdd = new List<DelimiterRow>
            {
                new DelimiterRow(new string?[] { "1", "2", "", null })
            };

        //add the list (range of rows)
        delimiterBuilder.AddRowRange(rowsToAdd);

        //what is the final output of creator
        var result = delimiterBuilder.WriteData();

        //split the text result into columns
        var splitTextResult = ParseResultsLazy(result).ToArray();

        //check the results
        Assert.Equal(1, splitTextResult[0].Value<int?>(0));
        Assert.Equal(2, splitTextResult[0].Value<int?>(1));
        Assert.Null(splitTextResult[0].Value<int?>(2));
        Assert.Null(splitTextResult[0].Value<int?>(3));
    }

    [Fact]
    public void CSVWithoutHeadersBulkLoadTest2()
    {
        //create the delimiter builder
        var delimiterBuilder = new DelimiterBuilder(CsvDelimiter);

        //create list of rows
        var rowsToAdd = new List<DelimiterRow>
            {
                new DelimiterRow(new string?[] { "1", "2", "3", "4" })
            };

        //add the list (range of rows)
        delimiterBuilder.AddRowRange(rowsToAdd);

        //what is the final output of creator
        var result = delimiterBuilder.WriteData();

        //split the text result into columns
        var splitTextResult = ParseResultsLazy(result).ToArray();

        //check the results
        Assert.Equal(1, splitTextResult[0].Value<int?>(0));
        Assert.Equal(2, splitTextResult[0].Value<int?>(1));
        Assert.Equal(3, splitTextResult[0].Value<int?>(2));
        Assert.Equal(4, splitTextResult[0].Value<int?>(3));
    }

    #endregion

    #region Without Headers With Multiple Rows

    [Fact]
    public void CSVWithoutHeadersMultiRowTest1()
    {
        //create the delimiter builder
        var delimiterBuilder = new DelimiterBuilder(CsvDelimiter);

        //add 2 rows to the builder
        delimiterBuilder.AddRow(new string?[] { "1", "2", "3", "4" });
        delimiterBuilder.AddRow(new string?[] { string.Empty, "6", null, "8" });

        //get the result
        var result = delimiterBuilder.WriteData();

        //split the text result into columns
        var splitTextResult = ParseResultsLazy(result).ToArray();

        //check the result now
        Assert.Equal(1, splitTextResult[0].Value<int?>(0));
        Assert.Equal(2, splitTextResult[0].Value<int?>(1));
        Assert.Equal(3, splitTextResult[0].Value<int?>(2));
        Assert.Equal(4, splitTextResult[0].Value<int?>(3));

        Assert.Null(splitTextResult[1].Value<int?>(0));
        Assert.Equal(6, splitTextResult[1].Value<int?>(1));
        Assert.Null(splitTextResult[1].Value<int?>(2));
        Assert.Equal(8, splitTextResult[1].Value<int?>(3));
    }

    #endregion

    #region Without Headers With Multiple Rows (Bulk Load)

    /// <summary>
    /// Test the delimiter using a csv format
    /// </summary>
    [Fact]
    public void CSVWithoutHeadersMultiRowBulkLoadTest1()
    {
        //create the delimiter builder
        var delimiterBuilder = new DelimiterBuilder(CsvDelimiter);

        //create list of rows
        var rowsToAdd = new List<DelimiterRow>
            {
                new DelimiterRow(new string?[] { "1", "2", "3", "4" }),
                new DelimiterRow(new string?[] { string.Empty, "6", null, "8" })
            };

        //add those rows to the builder
        delimiterBuilder.AddRowRange(rowsToAdd);

        //grab the resdult
        var result = delimiterBuilder.WriteData();

        //split the text result into columns
        var splitTextResult = ParseResultsLazy(result).ToArray();

        //check the result now
        Assert.Equal(1, splitTextResult[0].Value<int?>(0));
        Assert.Equal(2, splitTextResult[0].Value<int?>(1));
        Assert.Equal(3, splitTextResult[0].Value<int?>(2));
        Assert.Equal(4, splitTextResult[0].Value<int?>(3));

        Assert.Null(splitTextResult[1].Value<int?>(0));
        Assert.Equal(6, splitTextResult[1].Value<int?>(1));
        Assert.Null(splitTextResult[1].Value<int?>(2));
        Assert.Equal(8, splitTextResult[1].Value<int?>(3));
    }

    #endregion

    #region With Headers With A Single Line

    /// <summary>
    /// Test the delimiter using a csv format
    /// </summary>
    [Fact]
    public void CSVWithHeadersTest1()
    {
        //create the delimiter builder
        var delimiterBuilder = new DelimiterBuilder(["column1", "column2", "column3", "column4"], CsvDelimiter);

        //add a row
        delimiterBuilder.AddRow(new string?[] { "1", "2", "3", "4" });

        //grab the resdult
        var result = delimiterBuilder.WriteData();

        //split the text result into columns
        var splitTextResult = ParseResultsLazy(result).ToArray();

        //check the results
        Assert.Equal("column1", splitTextResult[0].Value<string>(0));
        Assert.Equal("column2", splitTextResult[0].Value<string>(1));
        Assert.Equal("column3", splitTextResult[0].Value<string>(2));
        Assert.Equal("column4", splitTextResult[0].Value<string>(3));

        Assert.Equal(1, splitTextResult[1].Value<int?>(0));
        Assert.Equal(2, splitTextResult[1].Value<int?>(1));
        Assert.Equal(3, splitTextResult[1].Value<int?>(2));
        Assert.Equal(4, splitTextResult[1].Value<int?>(3));
    }

    /// <summary>
    /// Test the delimiter using a csv format
    /// </summary>
    [Fact]
    public void CSVWithHeadersTest2()
    {
        //create the delimiter builder
        var delimiterBuilder = new DelimiterBuilder(["column1", "column2", "column3", "column4"], CsvDelimiter);

        //add a row
        delimiterBuilder.AddRow(new string?[] { "1", "2", null, string.Empty });

        //grab the resdult
        var result = delimiterBuilder.WriteData();

        //split the text result into columns
        var splitTextResult = ParseResultsLazy(result).ToArray();

        //check the results
        Assert.Equal("column1", splitTextResult[0].Value<string>(0));
        Assert.Equal("column2", splitTextResult[0].Value<string>(1));
        Assert.Equal("column3", splitTextResult[0].Value<string>(2));
        Assert.Equal("column4", splitTextResult[0].Value<string>(3));

        Assert.Equal(1, splitTextResult[1].Value<int?>(0));
        Assert.Equal(2, splitTextResult[1].Value<int?>(1));
        Assert.Null(splitTextResult[1].Value<int?>(2));
        Assert.Null(splitTextResult[1].Value<int?>(3));
    }

    #endregion

    #region With Headers With Muliple Line

    /// <summary>
    /// Test the delimiter using a csv format
    /// </summary>
    [Fact]
    public void CSVWithHeadersMultiRowTest1()
    {
        //create the delimiter builder
        var delimiterBuilder = new DelimiterBuilder(["column1", "column2", "column3", "column4"], CsvDelimiter);

        //add 2 rows
        delimiterBuilder.AddRow(new string?[] { "1", "2", "3", "4" });
        delimiterBuilder.AddRow(new string?[] { string.Empty, "6", null, "8" });

        //grab the resdult
        var result = delimiterBuilder.WriteData();

        //split the text result into columns
        var splitTextResult = ParseResultsLazy(result).ToArray();

        //check the results
        Assert.Equal("column1", splitTextResult[0].Value<string>(0));
        Assert.Equal("column2", splitTextResult[0].Value<string>(1));
        Assert.Equal("column3", splitTextResult[0].Value<string>(2));
        Assert.Equal("column4", splitTextResult[0].Value<string>(3));

        Assert.Equal(1, splitTextResult[1].Value<int?>(0));
        Assert.Equal(2, splitTextResult[1].Value<int?>(1));
        Assert.Equal(3, splitTextResult[1].Value<int?>(2));
        Assert.Equal(4, splitTextResult[1].Value<int?>(3));

        Assert.Null(splitTextResult[2].Value<int?>(0));
        Assert.Equal(6, splitTextResult[2].Value<int?>(1));
        Assert.Null(splitTextResult[2].Value<int?>(2));
        Assert.Equal(8, splitTextResult[2].Value<int?>(3));
    }

    #endregion

    #region With Headers With Muliple Line (Bulk Load)

    /// <summary>
    /// Test the delimiter using a csv format
    /// </summary>
    [Fact]
    public void CSVWithHeadersMultiRowBulkLoadTest1()
    {
        //create the delimiter builder
        var delimiterBuilder = new DelimiterBuilder(["column1", "column2", "column3", "column4"], CsvDelimiter);

        //create list of rows
        var rowsToAdd = new List<DelimiterRow>
            {
                //rowsToAdd 2 rows to the list
                new DelimiterRow(new string?[] { "1", "2", "3", "4" }),
                new DelimiterRow(new string?[] { string.Empty, "6", null, "8" })
            };

        //push those rows now
        delimiterBuilder.AddRowRange(rowsToAdd);

        //grab the resdult
        var result = delimiterBuilder.WriteData();

        //split the text result into columns
        var splitTextResult = ParseResultsLazy(result).ToArray();

        //check the results
        Assert.Equal("column1", splitTextResult[0].Value<string>(0));
        Assert.Equal("column2", splitTextResult[0].Value<string>(1));
        Assert.Equal("column3", splitTextResult[0].Value<string>(2));
        Assert.Equal("column4", splitTextResult[0].Value<string>(3));

        Assert.Equal(1, splitTextResult[1].Value<int?>(0));
        Assert.Equal(2, splitTextResult[1].Value<int?>(1));
        Assert.Equal(3, splitTextResult[1].Value<int?>(2));
        Assert.Equal(4, splitTextResult[1].Value<int?>(3));

        Assert.Null(splitTextResult[2].Value<int?>(0));
        Assert.Equal(6, splitTextResult[2].Value<int?>(1));
        Assert.Null(splitTextResult[2].Value<int?>(2));
        Assert.Equal(8, splitTextResult[2].Value<int?>(3));
    }

    #endregion
}
