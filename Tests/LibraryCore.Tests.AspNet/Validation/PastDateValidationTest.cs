using LibraryCore.AspNet.Validation;
using Xunit;

namespace LibraryCore.Tests.AspNet.Validation;

public class PastDateValidationTest
{
    [InlineData(null, true, false)]
    [InlineData("", true, false)]
    [InlineData("124/125/2049", true, false)]
    [InlineData("30/12/2018", true, false)]
    [InlineData("5/29/3050", true, false)]
    [InlineData("test124", true, false)]
    [InlineData("test/01/29", true, false)]
    [InlineData("!@/#$/#@$$", true, false)]

    [InlineData("03/04/1990", true, true)]
    [InlineData("12/19/1890", true, true)]
    [InlineData("5/31/1730", true, true)]
    [InlineData("2/29/2020", true, true)]

    [Theory]
    public void PastDateValidationAttribute(string dateToValidate, bool isRequiredField, bool isValidExpectedResult)
    {
        Assert.Equal(isValidExpectedResult, new PastDateValidationAttribute(isRequiredField).IsValid(dateToValidate));
    }
}
