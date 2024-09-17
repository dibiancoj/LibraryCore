using LibraryCore.Kafka.Serialization;

namespace LibraryCore.Tests.Kafka.Serialization;

public class NullableKafkaMessageSerializerTest
{
    public record MyModel(int Id, string Text);

    [Fact]
    public void Deserialize_WhenDataIsNull_ReturnsNotSuccessfulMessageSchema()
    {
        var serializer = new NullableKafkaMessageSerializer<MyModel>();

        var result = serializer.Deserialize([], true, default);

        Assert.False(result.WasDeserialized);
    }

    [Fact]
    public void Deserialize_WhenDataIsNotNull_ReturnsSuccessfulMessageSchema()
    {
        var serializer = new NullableKafkaMessageSerializer<MyModel>();

        var bytes = serializer.Serialize(new MyModel(99, "Jacob Trouba"), default);

        var result = serializer.Deserialize(bytes, true, default);

        if (!result.TryGetResult(out var model))
        {
            Assert.Fail();
        }

        Assert.Equal(99, model.Id);
        Assert.Equal("Jacob Trouba", model.Text);
    }
}