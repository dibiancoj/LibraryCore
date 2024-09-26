using LibraryCore.Core.Authentication;
using System.Text;

namespace LibraryCore.Tests.Core.Authentication;

public class BasicAuthenticationTest
{
    [Fact]
    public void BuildBasicAuthenticationHeaderValueTest()
    {
        const string userId = "PortalUser1";
        const string password = "Password123";

        Assert.Equal("UG9ydGFsVXNlcjE6UGFzc3dvcmQxMjM=", BasicAuthentication.ToBasicAuthenticationValue(userId, password));
    }

    [Fact]
    public void BuildBasicAuthenticationDecodeBack()
    {
        var (ClientId, ClientSecret) = BasicAuthentication.DecodeBasicAuthenticationHeaderValue("UG9ydGFsVXNlcjE6UGFzc3dvcmQxMjM=");

        Assert.Equal("PortalUser1", ClientId);
        Assert.Equal("Password123", ClientSecret);
    }

    [Fact]
    public void FullFlowTest()
    {
        const string userId = "PortalUser1";
        const string password = "Password123";

        var value = BasicAuthentication.ToBasicAuthenticationValue(userId, password);

        Assert.Equal("UG9ydGFsVXNlcjE6UGFzc3dvcmQxMjM=", value);

        var (ClientId, ClientSecret) = BasicAuthentication.DecodeBasicAuthenticationHeaderValue(value);

        Assert.Equal("PortalUser1", ClientId);
        Assert.Equal("Password123", ClientSecret);
    }

    [Fact]
    public void NoColon()
    {
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"myuserpassword"));

        var exceptionThrown = Assert.Throws<Exception>(() => BasicAuthentication.DecodeBasicAuthenticationHeaderValue(encoded));

        Assert.Equal("Invalid basic authentication header value. Value is missing the colon.", exceptionThrown.Message);
    }
}
