using LibraryCore.Core.Json.Converters;
using System.Text.Json;

namespace LibraryCore.Tests.Core.Json.Converters;

public class BooleanConverterTest
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void BooleanTestConverter(string value, bool expectedValue)
    {
        var json = $$"""
                      {
                        "Id" : 5,
                        "IsActive" : "{{value}}"
                      }
                   """;

        var settings = new JsonSerializerOptions();

        settings.Converters.Add(new BooleanConverter());

        var model = JsonSerializer.Deserialize<Model>(json, settings) ?? throw new Exception("Can't Deserialize Model");

        Assert.Equal(5, model.Id);
        Assert.Equal(expectedValue, model.IsActive);
    }

    public record Model(int Id, bool IsActive);
}
