using LibraryCore.Core.Delimiter;

namespace LibraryCore.Tests.Core.Delimiter;

public class DelimiterReaderTest
{
    [Fact]
    public void ParseFromLinesLazyTest()
    {
        var result = DelimiterReader.ParseFromLinesLazy(new[] { "abc|cde|fgh", "111|222|333" }, "|").ToArray();

        Assert.Equal(2, result.Length);

        Assert.Equal("abc", result[0].ColumnData[0]);
        Assert.Equal("cde", result[0].ColumnData[1]);
        Assert.Equal("fgh", result[0].ColumnData[2]);

        Assert.Equal("111", result[1].ColumnData[0]);
        Assert.Equal("222", result[1].ColumnData[1]);
        Assert.Equal("333", result[1].ColumnData[2]);
    }

    [Fact]
    public void ParseFromTextLinesLazyTest()
    {
        var result = DelimiterReader.ParseFromTextLinesLazy("abc|cde|fgh" + Environment.NewLine + "111|222|333", "|").ToArray();

        Assert.Equal(2, result.Length);

        Assert.Equal("abc", result[0].ColumnData[0]);
        Assert.Equal("cde", result[0].ColumnData[1]);
        Assert.Equal("fgh", result[0].ColumnData[2]);

        Assert.Equal("111", result[1].ColumnData[0]);
        Assert.Equal("222", result[1].ColumnData[1]);
        Assert.Equal("333", result[1].ColumnData[2]);
    }
}
