using LibraryCore.AspNet.Validation;

namespace LibraryCore.Tests.AspNet.Validation;

public class MinimumValueValidationTest
{
    [InlineData(null, false)] //default value
    [InlineData(0d, false)]
    [InlineData(.01d, true)]
    [InlineData(1.25d, true)]
    [InlineData(1.5d, true)]
    [Theory]
    public void MinValueValidation(double? valueToValidate, bool isValidExpectedResult)
    {
        Assert.Equal(isValidExpectedResult, new MinimumValueAttribute(0).IsValid(valueToValidate));
    }

    [InlineData(null, false, false)]
    [InlineData(null, true, true)]
    [Theory]
    public void MinValueAllowNullValidation(double? valueToValidate, bool allowNull, bool isValidExpectedResult)
    {
        Assert.Equal(isValidExpectedResult, new MinimumValueAttribute(0, allowNull).IsValid(valueToValidate));
    }
}
