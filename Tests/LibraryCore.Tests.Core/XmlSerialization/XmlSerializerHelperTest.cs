﻿using LibraryCore.Core.XmlSerialization;
using System.IO;
using Xunit;

namespace LibraryCore.Tests.Core.XmlSerialization
{
    public class XmlSerializerHelperTest
    {
        public class TestObject
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }

        [Fact]
        public void SerializeAndDeserializeXmlCorrect()
        {
            var xmlData = XMLSerializationHelper.SerializeObject(new TestObject { Id = 1, Text = "Test 1" });
            var backToObejct = XMLSerializationHelper.DeserializeObject<TestObject>(xmlData);

            Assert.Equal(1, backToObejct.Id);
            Assert.Equal("Test 1", backToObejct.Text);
        }

        [Fact]
        public void SerializeAndDeserializeToXElementXmlCorrect()
        {
            var xmlData = XMLSerializationHelper.SerializeObjectToXElement(new TestObject { Id = 1, Text = "Test 1" });
            var backToObejct = XMLSerializationHelper.DeserializeObject<TestObject>(xmlData);

            Assert.Equal(1, backToObejct.Id);
            Assert.Equal("Test 1", backToObejct.Text);
        }

        [Fact]
        public void DeserializeFromStream()
        {
            var xmlData = XMLSerializationHelper.SerializeObjectToXElement(new TestObject { Id = 9999, Text = "Test 9999" });

            using (var streamToUse = new MemoryStream())
            {
                xmlData.Save(streamToUse);

                //rewind the stream to the beg.
                streamToUse.Seek(0, SeekOrigin.Begin);

                var result = XMLSerializationHelper.DeserializeObject<TestObject>(streamToUse);

                Assert.Equal(9999, result.Id);
                Assert.Equal("Test 9999", result.Text);
            }
        }

    }
}