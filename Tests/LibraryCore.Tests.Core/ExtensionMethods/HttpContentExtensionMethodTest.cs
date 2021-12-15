﻿using LibraryCore.Core.ExtensionMethods;
using System.Text;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public class HttpContentExtensionMethodTest
{
    [Serializable]
    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    [Fact]
    public async Task ReadFromXmlAsyncTest()
    {
        var serializedObject = LibraryCore.Core.XmlSerialization.XMLSerializationHelper.SerializeObjectToXElement(new TestModel
        {
            Id = 100,
            Name = "One Hundred"
        });

        var content = new StringContent(serializedObject.ToString(), Encoding.UTF8, "text/xml");

        var result = await content.ReadFromXmlAsync<TestModel>() ?? throw new Exception("Can't Deserialize Model");

        Assert.Equal(100, result.Id); ;
        Assert.Equal("One Hundred", result.Name);
    }

}