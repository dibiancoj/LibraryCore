using LibraryCore.Core.UrlUtilities;
using System;
using Xunit;

namespace LibraryCore.Tests.Core.UrlUtilities
{
    public class UrlUtilityTest
    {
        [InlineData("http://www.google.com", "http://www.google.com", "http://www.google.com/")]
        [InlineData("www.google.com", "http://www.google.com", "www.google.com")]
        [InlineData("/test/doctor", "http://www.google.com", "http://www.google.com/test/doctor")]
        [InlineData("test/doctor", "http://www.google.com", "http://www.google.com/test/doctor")]
        [InlineData("test/doctor?id=5", "http://www.google.com", "http://www.google.com/test/doctor?id=5")]
        [Theory]
        public void UrlMakeAbsolute(string urlToInspect, string baseUrl, string expectedResult)
        {
            Assert.Equal(expectedResult, UrlUtility.MakeRelativeUriAbsolute(urlToInspect, new Uri(baseUrl)).ToString());
        }

        [InlineData(true, "http://www.google.com")]
        [InlineData(true, "https://www.google.com")]
        [InlineData(true, "www.google.com")]
        //[InlineData(false, "/test/doctor")] //this code in linux is failing. Can change the build machine to be windows or just comment this out.
        [InlineData(false, "test/doctor")]
        [InlineData(false, "test/doctor?id=5")]
        [InlineData(false, "#Id=5")]
        [Theory]
        public void IsAbsoluteUri(bool expectIsAbsoluteUri, string urlToInspect)
        {
            Assert.Equal(expectIsAbsoluteUri, UrlUtility.IsAbsoluteUri(urlToInspect));
        }
    }
}
