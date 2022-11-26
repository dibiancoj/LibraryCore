using LibraryCore.ApiClient;
using LibraryCore.IntegrationTests.Fixtures;
using System.Net.Http.Json;
using static LibraryCore.ApiClient.ContentTypeLookup;

namespace LibraryCore.IntegrationTests;

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
    public async Task HttpClientToJsonExtensionMethodTest()
    {
        var result = await WebApplicationFactoryFixture.HttpClientToUse.SendRequestToJsonAsync<ResultModel>(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleJsonPayload")) ?? throw new Exception("Can't deserialize result");

        Assert.Equal(9999, result.Id);
        Assert.Equal("1111", result.Text);
    }

    [Fact]
    public async Task HttpClientToXmlExtensionMethodTest()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleXmlPayload")
                                                                                     .AddAcceptType(AcceptTypeEnum.Xml));

        var result = await response.EnsureSuccessStatusCode()
                                .Content.ReadFromXmlAsync<XmlRoot>() ?? throw new Exception("Can't deserialize result");

        Assert.Equal(5, result.Id);
    }

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
    public async Task QueryStringsTest()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/QueryStringTest")
                                                                                        .AddQueryStrings(new Dictionary<string,string>
                                                                                        {
                                                                                            { "Q1", "One" },
                                                                                            { "Q2", "Two" }
                                                                                        }));

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
                                                                                        .AddFileStreamBody("formFiles", new KeyValuePair<string, byte[]>("file1.jpg", byteArray)));

        var result = await response.EnsureSuccessStatusCode()
                               .Content.ReadFromJsonAsync<IEnumerable<ResultModel>>() ?? throw new Exception("Can't deserialize result");

        Assert.Single(result);
        Assert.Contains(result, x => x.Id == 3 && x.Text == "file1.jpg");
    }

    [Fact]
    public async Task FileUploadWithStreamTest()
    {
        var streamOfFile1 = new MemoryStream(new byte[] { 1, 2, 3 });
        var streamOfFile2 = new MemoryStream(new byte[] { 4, 5, 6, 7 });

        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Post, "FluentHttpRequest/FileUploadStream")
                                                                                        .AddFileStreamBody("formFiles",
                                                                                                new KeyValuePair<string, Stream>("file1.jpg", streamOfFile1),
                                                                                                new KeyValuePair<string, Stream>("file2.jpg", streamOfFile2)));

        var result = await response.EnsureSuccessStatusCode()
                               .Content.ReadFromJsonAsync<IEnumerable<ResultModel>>() ?? throw new Exception("Can't deserialize result");

        Assert.Equal(2, result.Count());
        Assert.Contains(result, x => x.Id == 3 && x.Text == "file1.jpg");
        Assert.Contains(result, x => x.Id == 4 && x.Text == "file2.jpg");
    }

    #endregion

}