using LibraryCore.Core.ContentType;

namespace LibraryCore.Tests.Core.ContentType;

public class ContentTypeTest
{
    [InlineData("application/json", ContentTypeLookup.JsonContentType)]
    [InlineData("text/html", ContentTypeLookup.HtmlContentType)]
    [InlineData("text/plain", ContentTypeLookup.TextPlainContentType)]
    [InlineData("application/xml", ContentTypeLookup.XmlContentType)]
    [Theory(DisplayName = "Verify Correct Content Type")]
    public void VerifyContentTypes(string expectedValue, string testValue)
    {
        Assert.Equal(expectedValue, testValue);
    }

    [Fact]
    public void RetrieveAcceptTypeTest()
    {
        Assert.Equal(ContentTypeLookup.JsonMediaType, ContentTypeLookup.RetrieveAcceptType(ContentTypeLookup.AcceptTypeEnum.Json));
        Assert.Equal(ContentTypeLookup.XmlMediaType, ContentTypeLookup.RetrieveAcceptType(ContentTypeLookup.AcceptTypeEnum.Xml));
        Assert.Equal(ContentTypeLookup.TextPlainMediaType, ContentTypeLookup.RetrieveAcceptType(ContentTypeLookup.AcceptTypeEnum.TextPlain));
        Assert.Equal(ContentTypeLookup.TextHtmlMediaType, ContentTypeLookup.RetrieveAcceptType(ContentTypeLookup.AcceptTypeEnum.TextHtml));
    }

    [Fact]
    public void JsonMediaType()
    {
        Assert.Equal(ContentTypeLookup.JsonContentType, ContentTypeLookup.JsonMediaType.MediaType);
    }

    [Fact]
    public void HtmlMediaType()
    {
        Assert.Equal(ContentTypeLookup.HtmlContentType, ContentTypeLookup.TextHtmlMediaType.MediaType);
    }

    [Fact]
    public void TextMediaType()
    {
        Assert.Equal(ContentTypeLookup.TextPlainContentType, ContentTypeLookup.TextPlainMediaType.MediaType);
    }

    [Fact]
    public void XmlMediaType()
    {
        Assert.Equal(ContentTypeLookup.XmlContentType, ContentTypeLookup.XmlMediaType.MediaType);
    }
}