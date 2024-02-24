using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using LibraryCore.AwsSecretManager;
using LibraryCore.Core.ExtensionMethods;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryCore.Tests.AwsSecretManager;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SecretManagerUtilityTests.TestSecretWithJson))]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class AwsSecretJsonContext : JsonSerializerContext
{
}

public class SecretManagerUtilityTests
{
    [Fact]
    public async Task Non200Result()
    {
        var mockIAmazonSecretsManager = new Mock<IAmazonSecretsManager>();

        mockIAmazonSecretsManager.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new GetSecretValueResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.NotFound
            }));

        var exception = await Assert.ThrowsAsync<Exception>(() =>
        {
            return SecretManagerUtilities.GetSecretAsync(mockIAmazonSecretsManager.Object, "MySecretName");
        });

        mockIAmazonSecretsManager.VerifyAll();
    }

    [Fact]
    public async Task SuccessOnStringSecret()
    {
        var mockIAmazonSecretsManager = new Mock<IAmazonSecretsManager>();

        mockIAmazonSecretsManager.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new GetSecretValueResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                SecretString = "test123"
            }));

        Assert.Equal("test123", await SecretManagerUtilities.GetSecretAsync(mockIAmazonSecretsManager.Object, "MySecretName"));

        mockIAmazonSecretsManager.VerifyAll();
    }

    [Fact]
    public async Task SuccessOnBinarySecret()
    {
        var mockIAmazonSecretsManager = new Mock<IAmazonSecretsManager>();

        using var streamToMock = "test12345".ToBase64Encode().ToMemoryStream();

        mockIAmazonSecretsManager.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new GetSecretValueResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                SecretBinary = streamToMock
            }));

        Assert.Equal("test12345", await SecretManagerUtilities.GetSecretAsync(mockIAmazonSecretsManager.Object, "MySecretName"));

        mockIAmazonSecretsManager.VerifyAll();
    }

    [Fact]
    public async Task BothStringAndBinaryAreNull()
    {
        var mockIAmazonSecretsManager = new Mock<IAmazonSecretsManager>();

        mockIAmazonSecretsManager.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new GetSecretValueResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK
            }));

        Assert.Null(await SecretManagerUtilities.GetSecretAsync(mockIAmazonSecretsManager.Object, "MySecretName"));

        mockIAmazonSecretsManager.VerifyAll();
    }

    public record TestSecretWithJson(string Key, string Value, int Id);

    [Fact]
    public async Task SuccessOnJsonSecret()
    {
        var mockIAmazonSecretsManager = new Mock<IAmazonSecretsManager>();

        mockIAmazonSecretsManager.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new GetSecretValueResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                SecretString = JsonSerializer.Serialize(new TestSecretWithJson("DbKey", "123", 9999))
            }));

        var result = await SecretManagerUtilities.GetSecretAsync<TestSecretWithJson>(mockIAmazonSecretsManager.Object, "MySecretName");

        Assert.NotNull(result);
        Assert.Equal("DbKey", result.Key);
        Assert.Equal("123", result.Value);
        Assert.Equal(9999, result.Id);

        mockIAmazonSecretsManager.VerifyAll();
    }

    [Fact]
    public async Task SuccessOnNullJsonSecret()
    {
        var mockIAmazonSecretsManager = new Mock<IAmazonSecretsManager>();

        mockIAmazonSecretsManager.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new GetSecretValueResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                SecretString = string.Empty
            }));

        Assert.Null(await SecretManagerUtilities.GetSecretAsync<TestSecretWithJson>(mockIAmazonSecretsManager.Object, "MySecretName"));

        mockIAmazonSecretsManager.VerifyAll();
    }

    [Fact]
    public async Task SuccessOnKeyValuePair_string_string_Secret()
    {
        var mockSecretService = new Mock<IAmazonSecretsManager>();
        var arn = Guid.NewGuid().ToString();

        //using raw json ..to simulate as best as possible as we grab the json directly from aws
        const string jsonFromSecretManager = """
                                               {
                                                  "a": "aaaaa",
                                                  "b": "bbbbb"
                                               }
                                             """;

        mockSecretService.Setup(x => x.GetSecretValueAsync(It.Is<GetSecretValueRequest>(t => t.SecretId == arn && t.VersionStage == "AWSCURRENT"), default))
            .Returns(Task.FromResult(new GetSecretValueResponse
            {
                ARN = arn,
                SecretString = jsonFromSecretManager,
                HttpStatusCode = System.Net.HttpStatusCode.OK
            }));

        var secretFromService = await SecretManagerUtilities.GetSecretKeyValuePairAsync<string, string>(mockSecretService.Object, arn) ?? throw new Exception("Can't Get Value");

        Assert.Equal(2, secretFromService.Count);
        Assert.Contains(secretFromService, x => x.Key == "a" && x.Value == "aaaaa");
        Assert.Contains(secretFromService, x => x.Key == "b" && x.Value == "bbbbb");

        mockSecretService.VerifyAll();
    }

#if NET8_0_OR_GREATER

    [Fact]
    public async Task SuccessOnKeyValuePair_string_string_Secret_AOT()
    {
        var mockSecretService = new Mock<IAmazonSecretsManager>();
        var arn = Guid.NewGuid().ToString();

        //using raw json ..to simulate as best as possible as we grab the json directly from aws
        const string jsonFromSecretManager = """
                                               {
                                                  "a": "aaaaa",
                                                  "b": "bbbbb"
                                               }
                                             """;

        mockSecretService.Setup(x => x.GetSecretValueAsync(It.Is<GetSecretValueRequest>(t => t.SecretId == arn && t.VersionStage == "AWSCURRENT"), default))
            .Returns(Task.FromResult(new GetSecretValueResponse
            {
                ARN = arn,
                SecretString = jsonFromSecretManager,
                HttpStatusCode = System.Net.HttpStatusCode.OK
            }));

        var secretFromService = await SecretManagerUtilities.GetSecretKeyValuePairAsync<string, string>(mockSecretService.Object, arn, AwsSecretJsonContext.Default.DictionaryStringString) ?? throw new Exception("Can't Get Value");

        Assert.Equal(2, secretFromService.Count);
        Assert.Contains(secretFromService, x => x.Key == "a" && x.Value == "aaaaa");
        Assert.Contains(secretFromService, x => x.Key == "b" && x.Value == "bbbbb");

        mockSecretService.VerifyAll();
    }

    [Fact]
    public async Task SuccessOnJsonSecretWithAotOverload()
    {
        var mockIAmazonSecretsManager = new Mock<IAmazonSecretsManager>();

        mockIAmazonSecretsManager.Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new GetSecretValueResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK,
                SecretString = JsonSerializer.Serialize(new TestSecretWithJson("DbKey", "123", 9999))
            }));

        var result = await SecretManagerUtilities.GetSecretAsync<TestSecretWithJson>(mockIAmazonSecretsManager.Object, "MySecretName", AwsSecretJsonContext.Default.TestSecretWithJson);

        Assert.NotNull(result);
        Assert.Equal("DbKey", result.Key);
        Assert.Equal("123", result.Value);
        Assert.Equal(9999, result.Id);

        mockIAmazonSecretsManager.VerifyAll();
    }

#endif
}