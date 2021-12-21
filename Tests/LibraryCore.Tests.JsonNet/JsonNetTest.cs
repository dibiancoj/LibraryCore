using LibraryCore.Core.ExtensionMethods;
using LibraryCore.JsonNet;
using System.Text;

namespace LibraryCore.Tests.JsonNet;

public class JsonNetTest
{

    #region Test Object

    public record TestObject(int Id = 9999, string Text = "Test");

    #endregion

    #region Serialize To Byte Array And Back

    [Fact]
    public void JsonSerializeToByteArrayWithEmptyArray()
    {
        var settings = JsonSerializer.CreateDefault();
        TestObject? nullInstance = null;

        var serializedBytes = JsonNetUtilities.SerializeToUtf8Bytes(nullInstance, settings);

        Assert.Null(JsonNetUtilities.DeserializeFromByteArray<TestObject>(serializedBytes, settings));
    }

    [Fact]
    public void JsonSerializeToByteArrayAndBack()
    {
        var modelToSerialize = new TestObject();
        var settings = JsonSerializer.CreateDefault();

        var serializedBytes = JsonNetUtilities.SerializeToUtf8Bytes(modelToSerialize, settings);

        var deserializedBackToModel = JsonNetUtilities.DeserializeFromByteArray<TestObject>(serializedBytes, settings) ?? throw new Exception("Not Able To Deserialize");

        Assert.Equal(9999, deserializedBackToModel.Id);
        Assert.Equal("Test", deserializedBackToModel.Text);
    }

    #endregion

    #region Deserialization From Stream

    [Fact]
    public void JsonDeserializationFromStreamWhenJsonIsNull()
    {
        using var blankMemoryStream = new MemoryStream();

        Assert.Null(JsonNetUtilities.DeserializeFromStream<TestObject>(blankMemoryStream));
    }

    [Fact]
    public void JsonDeserializationFromStreamTest()
    {
        //create the dummy record
        var recordToTest = new TestObject();

        //let's serialize it into a json string
        using var jsonInStream = JsonConvert.SerializeObject(recordToTest).ToMemoryStream();

        //let's de-serialize it back from the stream
        var deserializedStringObject = JsonNetUtilities.DeserializeFromStream<TestObject>(jsonInStream) ?? throw new Exception("Not Able To Deserialize");

        //let's test the data
        Assert.NotNull(deserializedStringObject);

        //check the properties. check the id
        Assert.Equal(recordToTest.Id, deserializedStringObject.Id);

        //check the description
        Assert.Equal(recordToTest.Text, deserializedStringObject.Text);
    }

    [Fact]
    public void JsonDeserializationFromStreamTestOverloadWithSerializerWhenJsonIsNull()
    {
        //null memory stream. string.Empty.ToByteArray() would have the same result (should be null)
        using var blankMemoryStream = new MemoryStream();

        Assert.Null(JsonNetUtilities.DeserializeFromStream<TestObject>(blankMemoryStream, JsonSerializer.CreateDefault()));
    }

    [Fact]
    public void JsonDeserializationFromStreamTestOverloadWithSerializer()
    {
        //create the dummy record
        var recordToTest = new TestObject();

        //let's serialize it into a json string
        using var jsonInStream = JsonConvert.SerializeObject(recordToTest).ToMemoryStream();

        //let's de-serialize it back from the stream
        var deserializedStringObject = JsonNetUtilities.DeserializeFromStream<TestObject>(jsonInStream, JsonSerializer.CreateDefault()) ?? throw new Exception("Not Able To Deserialize");

        //let's test the data
        Assert.NotNull(deserializedStringObject);

        //check the properties. check the id
        Assert.Equal(recordToTest.Id, deserializedStringObject.Id);

        //check the description
        Assert.Equal(recordToTest.Text, deserializedStringObject.Text);
    }

    [Fact]
    public void JsonDeserializationFromStreamNonGenericVersionTestWhenJsonIsNull()
    {
        //null memory stream. string.Empty.ToByteArray() would have the same result (should be null)
        using var blankMemoryStream = new MemoryStream();

        Assert.Null(JsonNetUtilities.DeserializeFromStream(typeof(TestObject), blankMemoryStream, JsonSerializer.CreateDefault(new JsonSerializerSettings())) as TestObject);
    }

    [Fact]
    public void JsonDeserializationFromStreamNonGenericVersionTest()
    {
        //create the dummy record
        var recordToTest = new TestObject();

        //let's serialize it into a json string
        using var jsonInStream = JsonConvert.SerializeObject(recordToTest).ToMemoryStream();

        //let's de-serialize it back from the stream
        var deserializedStringObject = JsonNetUtilities.DeserializeFromStream(typeof(TestObject), jsonInStream, JsonSerializer.CreateDefault(new JsonSerializerSettings())) as TestObject ?? throw new Exception("Null Object");

        //let's test the data
        Assert.NotNull(deserializedStringObject);

        //check the properties. check the id
        Assert.Equal(recordToTest.Id, deserializedStringObject.Id);

        //check the description
        Assert.Equal(recordToTest.Text, deserializedStringObject.Text);
    }

    #endregion

    #region Serialize To Stream

    [Fact]
    public void JsonSerializeToStreamWithNullObject()
    {
        using var createdStream = JsonNetUtilities.SerializeToStream((TestObject?)null, JsonSerializer.CreateDefault());

        Assert.Null(JsonNetUtilities.DeserializeFromStream<TestObject>(createdStream.RewindAndConsumeStream()));
    }

    [Fact]
    public void JsonSerializeToStream()
    {
        var modelToSerialize = new TestObject();

        var expectedResult = JsonConvert.SerializeObject(modelToSerialize);

        using var createdStream = JsonNetUtilities.SerializeToStream(modelToSerialize, JsonSerializer.CreateDefault());

        Assert.Equal(expectedResult, Encoding.UTF8.GetString(createdStream.ToByteArray()));
    }

    [Fact]
    public void JsonSerializeAndDeserializeToStream()
    {
        var modelToSerialize = new TestObject();

        using var createdStream = JsonNetUtilities.SerializeToStream(modelToSerialize, JsonSerializer.CreateDefault());

        var deserializedObject = JsonNetUtilities.DeserializeFromStream<TestObject>(createdStream.RewindAndConsumeStream()) ?? throw new Exception("Not Able To Deserialize Model");

        Assert.Equal(modelToSerialize.Id, deserializedObject.Id);
        Assert.Equal(modelToSerialize.Text, deserializedObject.Text);
    }

    #endregion

    #region JObject From Stream

    [Fact]
    public async Task JObjectFromStreamTest()
    {
        //create the dummy record
        var recordToTest = new TestObject();

        //let's serialize it into a json string
        using var jsonInStream = JsonConvert.SerializeObject(recordToTest).ToMemoryStream();

        //let's de-serialize it back from the stream
        var jObject = await JsonNetUtilities.JObjectFromStreamAsync(jsonInStream);

        //let's test the data
        Assert.NotNull(jObject);

        //check the properties. check the id
        Assert.Equal(recordToTest.Id, jObject[nameof(TestObject.Id)]?.ToObject<int>());

        //check the description
        Assert.Equal(recordToTest.Text, jObject[nameof(TestObject.Text)]?.ToObject<string>());
    }

    #endregion

}
