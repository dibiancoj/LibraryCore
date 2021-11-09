using LibraryCore.AspNet.Attributes;
using System;
using System.Linq;
using Xunit;

namespace LibraryCore.Tests.AspNet.Attributes;

public class HttpGetOptionHeadAttributeTest
{
    //template overload constructor
    [InlineData("TemplateValue")]

    //no parameter constructor 
    [InlineData(null)]
    [Theory]
    public void VerifyCorrectHttpMethodsAreBeingUsed(string templateValue)
    {
        var supportedHttpTypes = templateValue == null ?
                                    new HttpGetOptionHeadAttribute().HttpMethods :
                                    new HttpGetOptionHeadAttribute(templateValue).HttpMethods;

        Assert.Equal(3, supportedHttpTypes.Count());
        Assert.Contains(supportedHttpTypes, x => x == "GET");
        Assert.Contains(supportedHttpTypes, x => x == "OPTIONS");
        Assert.Contains(supportedHttpTypes, x => x == "HEAD");
    }

    /// <summary>
    /// with template
    /// </summary>
    [Fact]
    public void VerifyThrowsOnNullTemplate()
    {
        Assert.Throws<ArgumentNullException>(() => new HttpGetOptionHeadAttribute(null).HttpMethods);
    }
}
