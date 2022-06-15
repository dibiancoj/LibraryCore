using LibraryCore.Core.Delimiter;

namespace LibraryCore.Tests.Core.Delimiter;

public class DelimiterReaderTest
{
    [Fact]
    public void ParseFromLinesLazyTest()
    {
        var result = DelimiterReader.ParseFromLinesLazy(new[] { @"""abc""|""cde""|""fgh""", @"""111""|""222""|""333""" }, '|').ToArray();

        Assert.Equal(2, result.Length);

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

        Assert.Equal("abc", result[0].Value<string>(0));
        Assert.Equal("cde", result[0].Value<string>(1));
        Assert.Equal("fgh", result[0].Value<string>(2));

        Assert.Equal(111, result[1].Value<int>(0));
        Assert.Equal(222, result[1].Value<int>(1));
        Assert.Equal(333, result[1].Value<int>(2));
    }

    [Fact]
    public void ParseWithDelimiterInString()
    {
        var result = DelimiterReader.ParseFromTextLinesLazy(@"""a|b|c""|""cde""|""fgh""", '|').ToArray();

        Assert.Single(result);

        Assert.Equal("a|b|c", result[0].Value<string>(0));
        Assert.Equal("cde", result[0].Value<string>(1));
        Assert.Equal("fgh", result[0].Value<string>(2));
    }

    [Fact]
    public void ParseWithQuoteInString()
    {
        var result = DelimiterReader.ParseFromTextLinesLazy(@"""ab""""c""|""def""|""zzz""", '|').ToArray();

        Assert.Single(result);

        Assert.Equal(@"ab""c", result[0].Value<string>(0));
        Assert.Equal("def", result[0].Value<string>(1));
        Assert.Equal("zzz", result[0].Value<string>(2));
    }
}
