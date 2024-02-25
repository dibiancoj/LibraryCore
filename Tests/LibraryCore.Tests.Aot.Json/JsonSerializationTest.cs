using LibraryCore.Aot.Json;
using System.Text.Json;

namespace LibraryCore.Tests.Aot.Json;

public class JsonSerializationTest
{
    public record TestModel(int Id, string Text);

    [Fact]
    public void NonAotWithGeneral()
    {
        var serializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.General);

        var json = JsonSerializer.Serialize(new TestModel(99, "99"), serializationOptions);

        var backToModel = JsonSerializer.Deserialize(json, ResolveJsonType.ResolveJsonTypeInfo<TestModel>())!;

        Assert.Equal(99, backToModel.Id);
        Assert.Equal("99", backToModel.Text);
    }

    [Fact]
    public void NonAotWithWeb()
    {
        var serializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var json = JsonSerializer.Serialize(new TestModel(88, "88"), serializationOptions);

        var backToModel = JsonSerializer.Deserialize(json, ResolveJsonType.ResolveJsonTypeInfo<TestModel>())!;

        Assert.Equal(88, backToModel.Id);
        Assert.Equal("88", backToModel.Text);
    }
}