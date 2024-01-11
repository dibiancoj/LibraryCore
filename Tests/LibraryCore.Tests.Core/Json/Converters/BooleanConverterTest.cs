using LibraryCore.Core.Json.Converters;
using System.Diagnostics.Contracts;
using System.Text.Json;

namespace LibraryCore.Tests.Core.Json.Converters;

public class BooleanConverterTest
{
    public BooleanConverterTest()
    {
        var settings = new JsonSerializerOptions();
        settings.Converters.Add(new BooleanConverter());

        Settings = settings;
    }

    private JsonSerializerOptions Settings { get; }

    [Theory]
    [InlineData("true", true)]
    [InlineData("TRUE", true)]
    [InlineData("tRuE", true)]
    [InlineData("trUE", true)]
    [InlineData("false", false)]
    [InlineData("FALSE", false)]
    [InlineData("FaLsE", false)]
    [InlineData("fALSE", false)]
    public void BooleanTestConverter(string value, bool expectedValue)
    {
        var json = $$"""
                      {
                        "Id" : 5,
                        "IsActive" : "{{value}}",
                        "UnitTestBool1": true,
                        "UnitTestBool2": false,
                        "UnitTestBool3": "abcShouldDefaultToFalse"
                      }
                   """;

        var model = JsonSerializer.Deserialize<Model>(json, Settings) ?? throw new Exception("Can't Deserialize Model");

        Assert.Equal(5, model.Id);
        Assert.Equal(expectedValue, model.IsActive);
        Assert.True(model.UnitTestBool1);
        Assert.False(model.UnitTestBool2);
        Assert.False(model.UnitTestBool3);
    }

    [Fact]
    public void WriteTest()
    {
        var model = new SmallerModel(true, false);

        var result = JsonSerializer.Serialize(model, Settings);
        var backToModel = JsonSerializer.Deserialize<SmallerModel>(result, Settings) ?? throw new Exception("Can't Deserialize Model");

        Assert.True(backToModel.IsActive);
        Assert.False(backToModel.IsActive2);
    }

    public record Model(int Id, bool IsActive, bool UnitTestBool1, bool UnitTestBool2, bool UnitTestBool3);
    public record SmallerModel(bool IsActive, bool IsActive2);
}
