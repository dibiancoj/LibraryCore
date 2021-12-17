using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.HttpRequestCore;
using LibraryCore.IntegrationTests.Fixtures;
using System.Net.Http.Json;
using static LibraryCore.Core.ContentType.ContentTypeLookup;

namespace LibraryCore.IntegrationTests
{
    public class FluentHttpRequestIntegrationTest : IClassFixture<WebApplicationFactoryFixture>
    {
        public WebApplicationFactoryFixture WebApplicationFactoryFixture { get; }

        public FluentHttpRequestIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture)
        {
            WebApplicationFactoryFixture = webApplicationFactoryFixture;
        }

        #region Models

        public record ResultModel(int Id, string Text);

        public class XmlRoot
        {
            public int Id { get; set; }
        }

        #endregion

        #region Tests

        [Fact]
        public async Task HeaderTest()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/HeaderTest")
                                                                                            .AddHeader("MyHeader", "My Value"));

            var result = await response.EnsureSuccessStatusCode()
                                    .Content.ReadAsStringAsync();

            Assert.Equal("My Value Result", result);
        }

        [Fact]
        public async Task QueryStringTest()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/QueryStringTest")
                                                                                            .AddQueryString("Q1", "One")
                                                                                            .AddQueryString("Q2", "Two"));

            var result = await response.EnsureSuccessStatusCode()
                                    .Content.ReadAsStringAsync();

            Assert.Equal("One:Two", result);
        }

        [Fact]
        public async Task SimpleJsonPayload()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleJsonPayload"));

            var result = await response.EnsureSuccessStatusCode()
                                    .Content.ReadFromJsonAsync<ResultModel>() ?? throw new Exception("Can't deserialize result");

            Assert.Equal(9999, result.Id);
            Assert.Equal("1111", result.Text);
        }

        [Fact]
        public async Task SimpleXmlPayload()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleXmlPayload")
                                                                                           .AddAcceptType(AcceptTypeEnum.Xml));

            var result = await response.EnsureSuccessStatusCode()
                                    .Content.ReadFromXmlAsync<XmlRoot>() ?? throw new Exception("Can't deserialize result");

            Assert.Equal(5, result.Id);
        }

        [Fact]
        public async Task SimpleJsonPayloadWithJsonRequestParameters()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleJsonPayloadWithJsonParameters")
                                                                                            .AddJsonBody(new
                                                                                            {
                                                                                                Id = 5,
                                                                                                Text = "5"
                                                                                            }));

            var result = await response.EnsureSuccessStatusCode()
                                    .Content.ReadFromJsonAsync<ResultModel>() ?? throw new Exception("Can't deserialize result");

            Assert.Equal(6, result.Id);
            Assert.Equal("5_Result", result.Text);
        }

        [Fact]
        public async Task FormsEncodedParameters()
        {
            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/FormsEncodedParameters")
                                                                                            .AddFormsUrlEncodedBody(new[]
                                                                                            {
                                                                                                new KeyValuePair<string,string>("4", "4"),
                                                                                                new KeyValuePair<string,string>("5", "5")
                                                                                            }));

            var result = await response.EnsureSuccessStatusCode()
                                    .Content.ReadFromJsonAsync<IEnumerable<ResultModel>>() ?? throw new Exception("Can't deserialize result");

            Assert.Equal(2, result.Count());

            Assert.Contains(result, x => x.Id == 14 && x.Text == "4_Result");
            Assert.Contains(result, x => x.Id == 15 && x.Text == "5_Result");
        }

        [Fact]
        public async Task FileUploadWithByteArrayTest()
        {
            var byteArray = new byte[] { 1, 2, 3 };

            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Post, "FluentHttpRequest/FileUploadStream")
                                                                                            .AddFileStreamBody("file1.jpg", "formFiles", byteArray));

            var result = await response.EnsureSuccessStatusCode()
                                   .Content.ReadFromJsonAsync<IEnumerable<ResultModel>>() ?? throw new Exception("Can't deserialize result");

            Assert.Single(result);
            Assert.Contains(result, x => x.Id == 3 && x.Text == "file1.jpg");
        }

        [Fact]
        public async Task FileUploadWithStreamTest()
        {
            var streamOfFile = new MemoryStream(new byte[] { 1, 2, 3 });

            var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Post, "FluentHttpRequest/FileUploadStream")
                                                                                            .AddFileStreamBody("file1.jpg", "formFiles", streamOfFile));

            var result = await response.EnsureSuccessStatusCode()
                                   .Content.ReadFromJsonAsync<IEnumerable<ResultModel>>() ?? throw new Exception("Can't deserialize result");

            Assert.Single(result);
            Assert.Contains(result, x => x.Id == 3 && x.Text == "file1.jpg");
        }

        #endregion

    }
}
