using LibraryCore.XmlSerialization;

namespace LIbraryCore.Tests.XmlSerialization;

public class XmlSerializerHelperTest
{
    public class TestObject
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
    }

    [Fact]
    public void SerializeAndDeserializeXmlCorrectWhenNullObject()
    {
        Assert.Null(XMLSerializationHelper.DeserializeObject<TestObject>(XMLSerializationHelper.SerializeObject((TestObject?)null)));
    }

    [Fact]
    public void SerializeAndDeserializeXmlCorrect()
    {
        var xmlData = XMLSerializationHelper.SerializeObject(new TestObject { Id = 1, Text = "Test 1" });
        var backToObejct = XMLSerializationHelper.DeserializeObject<TestObject>(xmlData) ?? throw new Exception("Not Able To Deserialize Mode");

        Assert.Equal(1, backToObejct.Id);
        Assert.Equal("Test 1", backToObejct.Text);
    }

    [Fact]
    public void SerializeAndDeserializeToXElementXmlCorrectWhenNullObject()
    {
        Assert.Null(XMLSerializationHelper.DeserializeObject<TestObject>(XMLSerializationHelper.SerializeObjectToXElement((TestObject?)null)));
    }

    [Fact]
    public void SerializeAndDeserializeToXElementXmlCorrect()
    {
        var xmlData = XMLSerializationHelper.SerializeObjectToXElement(new TestObject { Id = 1, Text = "Test 1" });
        var backToObejct = XMLSerializationHelper.DeserializeObject<TestObject>(xmlData) ?? throw new Exception("Not Able To Deserialize Mode");

        Assert.Equal(1, backToObejct.Id);
        Assert.Equal("Test 1", backToObejct.Text);
    }

    [Fact]
    public void SerializeAndDeserializeFromStreamWhenNull()
    {
        var xmlData = XMLSerializationHelper.SerializeObjectToXElement((TestObject?)null);

        using var streamToUse = new MemoryStream();

        xmlData.Save(streamToUse);

        //rewind the stream to the beg.
        streamToUse.Seek(0, SeekOrigin.Begin);

        Assert.Null(XMLSerializationHelper.DeserializeObject<TestObject>(streamToUse));
    }

    [Fact]
    public void SerializeAndDeserializeFromStream()
    {
        var xmlData = XMLSerializationHelper.SerializeObjectToXElement(new TestObject { Id = 9999, Text = "Test 9999" });

        using var streamToUse = new MemoryStream();

        xmlData.Save(streamToUse);

        //rewind the stream to the beg.
        streamToUse.Seek(0, SeekOrigin.Begin);

        var result = XMLSerializationHelper.DeserializeObject<TestObject>(streamToUse) ?? throw new Exception("Not Able To Deserialize Mode");

        Assert.Equal(9999, result.Id);
        Assert.Equal("Test 9999", result.Text);
    }

}