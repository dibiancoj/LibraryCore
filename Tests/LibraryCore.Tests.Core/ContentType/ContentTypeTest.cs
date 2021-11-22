using LibraryCore.Core.ContentType;

namespace LibraryCore.Tests.Core.ContentType;

public class ContentTypeTest
{
    [InlineData("application/json", ContentTypeLookup.JsonContentType)]
    [InlineData("text/html", ContentTypeLookup.HtmlContentType)]
    [InlineData("text/plain", ContentTypeLookup.TextPlainContentType)]
    [Theory(DisplayName = "Verify Correct Content Type")]
    public void VerifyContentTypes(string expectedValue, string testValue)
    {
        Assert.Equal(expectedValue, testValue);
    }

    [Fact]
    public void JsonMedicaType()
    {
        Assert.Equal(ContentTypeLookup.JsonContentType, ContentTypeLookup.JsonMediaType.MediaType);
    }

    [Fact]
    public void HtmlMedicaType()
    {
        Assert.Equal(ContentTypeLookup.HtmlContentType, ContentTypeLookup.HtmlMediaType.MediaType);
    }

    [Fact]
    public void TextMedicaType()
    {
        Assert.Equal(ContentTypeLookup.TextPlainContentType, ContentTypeLookup.TextMediaType.MediaType);
    }
}
