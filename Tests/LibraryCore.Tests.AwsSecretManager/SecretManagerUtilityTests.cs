using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using LibraryCore.AwsSecretManager;
using LibraryCore.Core.ExtensionMethods;

namespace LibraryCore.Tests.AwsSecretManager;

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
}