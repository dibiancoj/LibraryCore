using LibraryCore.Core.DataTypes;

namespace LibraryCore.Tests.Core.DataTypes;

public class MutableKeyValuePairTest
{
    [Fact]
    public void PropertiesWithoutConstructorSettingAreCorrect()
    {
        var model = new MutableKeyValuePair<string, string>
        {
            Key = "key1",
            Value = "value1"
        };

        Assert.Equal("key1", model.Key);
        Assert.Equal("value1", model.Value);
    }

    [Fact]
    public void PropertiesAreCorrect()
    {
        var model = new MutableKeyValuePair<string, string>("key1", "value1");

        Assert.Equal("key1", model.Key);
        Assert.Equal("value1", model.Value);
    }
}
