using LibraryCore.Core.Delimiter;
using LibraryCore.Core.ExtensionMethods;

namespace LibraryCore.Tests.Core.Delimiter;

public class DelimiterReaderTest
{

    private static void AssertColumnCount(IEnumerable<DelimiterBuilder.DelimiterRow> result, int expectedColumnCount)
    {
        result.ForEach(row =>
        {
            Assert.Equal(expectedColumnCount, row.ColumnData.Count);
        });
    }

    [Fact]
    public void ParseFromLinesLazyTest()
    {
        var result = DelimiterReader.ParseFromLinesLazy(new[] { @"""abc""|""cde""|""fgh""", @"""111""|""222""|""333""" }, '|').ToArray();

        Assert.Equal(2, result.Length);
        AssertColumnCount(result, 3);

        Assert.Equal("abc", result[0].Value<string>(0));
        Assert.Equal("cde", result[0].Value<string>(1));
        Assert.Equal("fgh", result[0].Value<string>(2));

        Assert.Equal(111, result[1].Value<int>(0));
        Assert.Equal(222, result[1].Value<int>(1));
        Assert.Equal(333, result[1].Value<int>(2));
    }

    [Fact]
    public void ParseFromTextLinesLazyTest()
    {
        var result = DelimiterReader.ParseFromTextLinesLazy(@"""abc""|""cde""|""fgh""" + Environment.NewLine + @"""111""|""222""|""333""", '|').ToArray();

        Assert.Equal(2, result.Length);
        AssertColumnCount(result, 3);

        Assert.Equal("abc", result[0].Value<string>(0));
        Assert.Equal("cde", result[0].Value<string>(1));
        Assert.Equal("fgh", result[0].Value<string>(2));

        Assert.Equal(111, result[1].Value<int>(0));
        Assert.Equal(222, result[1].Value<int>(1));
        Assert.Equal(333, result[1].Value<int>(2));
    }

    [Fact]
    public void ParseWithNullAtEnd()
    {
        var result = DelimiterReader.ParseFromTextLinesLazy(@"""abc"",""cde"",", ',').ToArray();

        AssertColumnCount(result, 3);

        Assert.Equal("abc", result[0].Value<string>(0));
        Assert.Equal("cde", result[0].Value<string>(1));
        Assert.Null(result[0].Value<string?>(2));
    }

    [Fact]
    public void ParseWithNullAtStart()
    {
        var result = DelimiterReader.ParseFromTextLinesLazy(@",""cde"",""xyz""", ',').ToArray();

        AssertColumnCount(result, 3);

        Assert.Null(result[0].Value<string>(0));
        Assert.Equal("cde", result[0].Value<string>(1));
        Assert.Equal("xyz", result[0].Value<string>(2));
    }

    [Fact]
    public void ParseWithDelimiterInString()
    {
        var result = DelimiterReader.ParseFromTextLinesLazy(@"""a|b|c""|""cde""|""fgh""", '|').ToArray();

        AssertColumnCount(result, 3);
        Assert.Single(result);

        Assert.Equal("a|b|c", result[0].Value<string>(0));
        Assert.Equal("cde", result[0].Value<string>(1));
        Assert.Equal("fgh", result[0].Value<string>(2));
    }

    [Fact]
    public void ParseWithQuoteInString()
    {
        var result = DelimiterReader.ParseFromTextLinesLazy(@"""ab""""c""|""def""|""zzz""", '|').ToArray();

        AssertColumnCount(result, 3);
        Assert.Single(result);

        Assert.Equal(@"ab""c", result[0].Value<string>(0));
        Assert.Equal("def", result[0].Value<string>(1));
        Assert.Equal("zzz", result[0].Value<string>(2));
    }

    [Fact]
    public void ParseWithWhiteSpace()
    {
        var result = DelimiterReader.ParseFromTextLinesLazy(@"""ab""""c""|""def""|""zzz""", '|').ToArray();

        AssertColumnCount(result, 3);
        Assert.Single(result);

        Assert.Equal(@"ab""c", result[0].Value<string>(0));
        Assert.Equal("def", result[0].Value<string>(1));
        Assert.Equal("zzz", result[0].Value<string>(2));
    }

    [InlineData("aa|bb|cc", "aa", "bb", "cc")]
    [InlineData("aa|bb|", "aa", "bb", null)]
    [InlineData("|bb|cc", null, "bb", "cc")]
    [Theory]
    public void ParseWithNoQuotes(string stringToParse, string? expected0, string? expected1, string? expected2)
    {
        var result = DelimiterReader.ParseFromTextLinesLazy(stringToParse, '|').ToArray();

        AssertColumnCount(result, 3);
        Assert.Single(result);

        Assert.Equal(expected0, result[0].Value<string>(0));
        Assert.Equal(expected1, result[0].Value<string>(1));
        Assert.Equal(expected2, result[0].Value<string>(2));
    }

    [InlineData(@"aa|""bb""|cc")]
    [InlineData(@"aa|""bb""|""cc""")]
    [InlineData(@"""aa""|""bb""|cc")]
    [Theory]
    public void ParseWithMixedQuotes(string stringToParse)
    {
        var result = DelimiterReader.ParseFromTextLinesLazy(stringToParse, '|').ToArray();

        AssertColumnCount(result, 3);
        Assert.Single(result);

        Assert.Equal("aa", result[0].Value<string>(0));
        Assert.Equal("bb", result[0].Value<string>(1));
        Assert.Equal("cc", result[0].Value<string>(2));
    }
}
