using LibraryCore.JsonNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LibraryCore.Tests.JsonNet
{
    public class JsonNetTest
    {

        #region Framework

        private MemoryStream ToStream(string stringToWriteIntoAStream)
        {
            //can't dispose of anything otherwise you won't be able to read it...The calling method needs to make sure they dispose of the stream

            //create the memory stream
            var memoryStreamToUse = new MemoryStream();

            //create the writer
            var writerToUse = new StreamWriter(memoryStreamToUse);

            //write the string data
            writerToUse.Write(stringToWriteIntoAStream);

            //flush it out
            writerToUse.Flush();

            //set the position to the beg of the stream
            memoryStreamToUse.Position = 0;

            //go run the test method
            return memoryStreamToUse;
        }

        #endregion

        #region Test Object

        public record TestObject(int Id = 9999, string Text="Test");

        #endregion

        #region Serialize To Byte Array And Back

        [Fact]
        public void JsonSerializeToByteArrayAndBack()
        {
            var modelToSerialize = new TestObject();
            var settings = JsonSerializer.CreateDefault();

            var serializedBytes = JsonNetUtilities.SerializeToUtf8Bytes(modelToSerialize, settings);

            var deserializedBackToModel = JsonNetUtilities.DeserializeFromByteArray<TestObject>(serializedBytes, settings);

            Assert.Equal(9999, deserializedBackToModel.Id);
            Assert.Equal("Test", deserializedBackToModel.Text);
        }

        #endregion

        #region Deserialization From Stream

        [Fact]
        public void JsonDeserializationFromStreamTest()
        {
            //create the dummy record
            var recordToTest = new TestObject();

            //let's serialize it into a json string
            var jsonInStream = ToStream(JsonConvert.SerializeObject(recordToTest));

            //let's de-serialize it back from the stream
            var deserializedStringObject = JsonNetUtilities.DeserializeFromStream<TestObject>(jsonInStream);

            //let's test the data
            Assert.NotNull(deserializedStringObject);

            //check the properties. check the id
            Assert.Equal(recordToTest.Id, deserializedStringObject.Id);

            //check the description
            Assert.Equal(recordToTest.Text, deserializedStringObject.Text);
        }

        [Fact]
        public void JsonDeserializationFromStreamTestOverloadWithSerializer()
        {
            //create the dummy record
            var recordToTest = new TestObject();

            //let's serialize it into a json string
            var jsonInStream = ToStream(JsonConvert.SerializeObject(recordToTest));

            //let's de-serialize it back from the stream
            var deserializedStringObject = JsonNetUtilities.DeserializeFromStream<TestObject>(jsonInStream, JsonSerializer.CreateDefault());

            //let's test the data
            Assert.NotNull(deserializedStringObject);

            //check the properties. check the id
            Assert.Equal(recordToTest.Id, deserializedStringObject.Id);

            //check the description
            Assert.Equal(recordToTest.Text, deserializedStringObject.Text);
        }

        [Fact]
        public void JsonDeserializationFromStreamNonGenericVersionTest()
        {
            //create the dummy record
            var recordToTest = new TestObject();

            //let's serialize it into a json string
            using var jsonInStream = ToStream(JsonConvert.SerializeObject(recordToTest));

            //let's de-serialize it back from the stream
            var deserializedStringObject = JsonNetUtilities.DeserializeFromStream(typeof(TestObject), jsonInStream, JsonSerializer.CreateDefault(new JsonSerializerSettings())) as TestObject;

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
        public void JsonSerializeToStream()
        {
            var modelToSerialize = new TestObject();

            var expectedResult = JsonConvert.SerializeObject(modelToSerialize);

            using var createdStream = JsonNetUtilities.SerializeToStream(modelToSerialize, JsonSerializer.CreateDefault());

            Assert.Equal(expectedResult, Encoding.UTF8.GetString(createdStream.ToArray()));
        }

        #endregion

        #region JObject From Stream

        [Fact]
        public async Task JObjectFromStreamTest()
        {
            //create the dummy record
            var recordToTest = new TestObject();

            //let's serialize it into a json string
            var jsonInStream = ToStream(JsonConvert.SerializeObject(recordToTest));

            //let's de-serialize it back from the stream
            JObject jObject = await JsonNetUtilities.JObjectFromStreamAsync(jsonInStream);

            //let's test the data
            Assert.NotNull(jObject);

            //check the properties. check the id
            Assert.Equal(recordToTest.Id, jObject[nameof(TestObject.Id)].ToObject<int>());

            //check the description
            Assert.Equal(recordToTest.Text, jObject[nameof(TestObject.Text)].ToObject<string>());
        }

        #endregion

    }
}
