using LibraryCore.AspNet.Validation;

namespace LibraryCore.Tests.AspNet.Validation;

public class ZipCodeValidationTest
{
    [InlineData(null, true, false)]
    [InlineData("0", true, false)]
    [InlineData("15", true, false)]
    [InlineData("464", true, false)]
    [InlineData("0000", true, false)]
    [InlineData("123244", true, false)]
    [InlineData("a", true, false)]
    [InlineData("!4$&#", true, false)]

    [InlineData("27541", true, true)]
    [InlineData("01475", true, true)]
    [InlineData("67254", true, true)]
    [InlineData("07842", true, true)]

    [Theory]
    public void ZipCodeValidationAttribute(string? zipToValidate, bool isRequiredField, bool isValidExpectedResult)
    {
        Assert.Equal(isValidExpectedResult, new USZipCodeValidationAttribute(isRequiredField).IsValid(zipToValidate));
    }
}
