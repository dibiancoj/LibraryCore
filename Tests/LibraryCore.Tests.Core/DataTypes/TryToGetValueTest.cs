using LibraryCore.Core.DataTypes;

namespace LibraryCore.Tests.Core.DataTypes;

public class TryToGetValueTest
{
    [Fact]
    public void NotFound()
    {
        var result = new TryToGetValue<string>(false, null);

        Assert.False(result.ItemWasFound);
        Assert.Null(result.ItemValueIfFound);

        Assert.False(result.GetValueIfFound(out var itemIfFound));
        Assert.Null(itemIfFound);
    }

    [Fact]
    public void WasFound()
    {
        var result = new TryToGetValue<string>(true, "test123");

        Assert.True(result.ItemWasFound);
        Assert.Equal("test123", result.ItemValueIfFound);

        Assert.True(result.GetValueIfFound(out var itemIfFound));
        Assert.Equal("test123", itemIfFound);
    }
}