using LibraryCore.Core.DataTypes;

namespace LibraryCore.Tests.Core.DataTypes;

public class TryToGetValueResultTest
{
    [Fact]
    public void NotFound()
    {
        var result = new TryToGetValueResult<string>(false, null);

        Assert.False(result.ItemWasFound);
        Assert.Null(result.ItemValueIfFound);

        Assert.False(result.GetValueIfFound(out var itemIfFound));
        Assert.Null(itemIfFound);
    }

    [Fact]
    public void WasFound()
    {
        var result = new TryToGetValueResult<string>(true, "test123");

        Assert.True(result.ItemWasFound);
        Assert.Equal("test123", result.ItemValueIfFound);

        Assert.True(result.GetValueIfFound(out var itemIfFound));
        Assert.Equal("test123", itemIfFound);
    }
}