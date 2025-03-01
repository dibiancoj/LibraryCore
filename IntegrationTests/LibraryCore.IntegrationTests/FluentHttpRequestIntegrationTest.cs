﻿using LibraryCore.ApiClient;
using LibraryCore.ApiClient.ExtensionMethods;
using LibraryCore.IntegrationTests.Fixtures;
using LibraryCore.Shared;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static LibraryCore.ApiClient.ContentTypeLookup;

namespace LibraryCore.IntegrationTests;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(TestAotModel))]
internal partial class AotJsonContext : JsonSerializerContext
{
}

public record TestAotModel(int Id, string Text);

public class FluentHttpRequestIntegrationTest(WebApplicationFactoryFixture webApplicationFactoryFixture) : IClassFixture<WebApplicationFactoryFixture>
{
    public WebApplicationFactoryFixture WebApplicationFactoryFixture { get; } = webApplicationFactoryFixture;

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
        var result = await WebApplicationFactoryFixture.HttpClientToUse.SendRequestToJsonAsync<ResultModel>(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleJsonPayload"), cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal(9999, result.Id);
        Assert.Equal("1111", result.Text);
    }

    [Fact]
    public async Task HttpClientToXmlExtensionMethodTest()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleXmlPayload")
                                                                                     .AddAcceptType(AcceptTypeEnum.Xml), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                                .Content.ReadFromXmlAsync<XmlRoot>(cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal(5, result.Id);
    }

    [Fact]
    public async Task HeaderTest()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/HeaderTest")
                                                                                        .AddHeader("MyHeader", "My Value"), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                                .Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("My Value Result", result);
    }

    [Fact]
    public async Task QueryStringTest()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/QueryStringTest")
                                                                                        .AddQueryString("Q1", "One")
                                                                                        .AddQueryString("Q2", "Two"), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                                .Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("One:Two", result);
    }

    [Fact]
    public async Task QueryStringsTest()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/QueryStringTest")
                                                                                        .AddQueryStrings(new Dictionary<string, string?>
                                                                                        {
                                                                                            { "Q1", "One" },
                                                                                            { "Q2", "Two" }
                                                                                        }), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                                .Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("One:Two", result);
    }

    [Fact]
    public async Task SimpleJsonPayload()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleJsonPayload"), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                                .Content.ReadFromJsonAsync<ResultModel>(cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal(9999, result.Id);
        Assert.Equal("1111", result.Text);
    }

    [Fact]
    public async Task SimpleXmlPayload()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleXmlPayload")
                                                                                       .AddAcceptType(AcceptTypeEnum.Xml), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                                .Content.ReadFromXmlAsync<XmlRoot>(cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

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
                                                                                        }), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                                .Content.ReadFromJsonAsync<ResultModel>(cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal(6, result.Id);
        Assert.Equal("5_Result", result.Text);
    }

    [Trait(ErrorMessages.AotUnitTestTraitName, ErrorMessages.AotUnitTestTraitValue)]
    [Fact]
    public async Task SimpleJsonPayloadWithJsonRequestParameters_WithAot()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleJsonPayloadWithJsonParameters")
                                                                                        .AddJsonBody(new TestAotModel(5, "5"), AotJsonContext.Default.TestAotModel), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                                .Content.ReadFromJsonAsync<ResultModel>(cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal(6, result.Id);
        Assert.Equal("5_Result", result.Text);
    }

    [Fact]
    public async Task SimpleJsonPayloadWithJsonOptions()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/SimpleJsonPayloadWithJsonParameters")
                                                                                        .AddJsonBody(new
                                                                                        {
                                                                                            id = 5,
                                                                                            text = "5"
                                                                                        }, jsonSerializerOptions: new JsonSerializerOptions
                                                                                        {
                                                                                            PropertyNameCaseInsensitive = true
                                                                                        }), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                                .Content.ReadFromJsonAsync<ResultModel>(cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal(6, result.Id);
        Assert.Equal("5_Result", result.Text);
    }

    [Fact]
    public async Task FormsEncodedParameters()
    {
        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Get, "FluentHttpRequest/FormsEncodedParameters")
                                                                                        .AddFormsUrlEncodedBody(
                                                                                        [
                                                                                            new KeyValuePair<string, string>("4", "4"),
                                                                                            new KeyValuePair<string, string>("5", "5")
                                                                                        ]), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                                .Content.ReadFromJsonAsync<IEnumerable<ResultModel>>(cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal(2, result.Count());

        Assert.Contains(result, x => x.Id == 14 && x.Text == "4_Result");
        Assert.Contains(result, x => x.Id == 15 && x.Text == "5_Result");
    }

    [Fact]
    public async Task FileUploadWithByteArrayTest()
    {
        var byteArray = new byte[] { 1, 2, 3 };

        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Post, "FluentHttpRequest/FileUploadStream")
                                                                                        .AddFileStreamBody("formFiles", new KeyValuePair<string, byte[]>("file1.jpg", byteArray)), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                               .Content.ReadFromJsonAsync<IEnumerable<ResultModel>>(cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Single(result);
        Assert.Contains(result, x => x.Id == 3 && x.Text == "file1.jpg");
    }

    [Fact]
    public async Task FileUploadWithStreamTest()
    {
        var streamOfFile1 = new MemoryStream([1, 2, 3]);
        var streamOfFile2 = new MemoryStream([4, 5, 6, 7]);

        var response = await WebApplicationFactoryFixture.HttpClientToUse.SendAsync(new FluentRequest(HttpMethod.Post, "FluentHttpRequest/FileUploadStream")
                                                                                        .AddFileStreamBody("formFiles",
                                                                                                new KeyValuePair<string, Stream>("file1.jpg", streamOfFile1),
                                                                                                new KeyValuePair<string, Stream>("file2.jpg", streamOfFile2)), TestContext.Current.CancellationToken);

        var result = await response.EnsureSuccessStatusCode()
                               .Content.ReadFromJsonAsync<IEnumerable<ResultModel>>(cancellationToken: TestContext.Current.CancellationToken) ?? throw new Exception("Can't deserialize result");

        Assert.Equal(2, result.Count());
        Assert.Contains(result, x => x.Id == 3 && x.Text == "file1.jpg");
        Assert.Contains(result, x => x.Id == 4 && x.Text == "file2.jpg");
    }

    #endregion

}