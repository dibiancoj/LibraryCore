using LibraryCore.Kafka.Models;

namespace LibraryCore.Tests.Kafka.Models;

public class KafkaNullableOfTTest
{
    public record Model(int Id);

    [Fact]
    public void SuccessfulMessageSchema_WhenModelIsNull_ReturnsNotSuccessfulMessageSchema()
    {
        var result = KafkaNullableOfT<Model>.SuccessfulMessageSchema(null);

        Assert.False(result.TryGetResult(out var tryGetResult));
        Assert.Null(tryGetResult);
    }

    [Fact]
    public void SuccessfulMessageSchema_WhenModelIsNotNull_ReturnsNotSuccessfulMessageSchema()
    {
        var result = KafkaNullableOfT<Model>.SuccessfulMessageSchema(new Model(24));

        Assert.True(result.TryGetResult(out var tryGetResult));
        Assert.Equal(24, tryGetResult.Id);
    }

    [Fact]
    public void NotSuccessful()
    {
        var result = KafkaNullableOfT<Model>.NotSuccessfulMessageSchema();

        Assert.False(result.TryGetResult(out var tryGetResult));
        Assert.Null(tryGetResult);
    }
}
