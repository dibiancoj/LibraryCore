using LibraryCore.AspNet.Validation;

namespace LibraryCore.Tests.AspNet.Validation;

public class MaximumValueValidationTest
{
    [InlineData(null, false)]
    [InlineData(0, true)]
    [InlineData(.90, true)]
    [InlineData(1.04, true)]
    [InlineData(1.05, true)]
    [InlineData(1.06, false)]
    [InlineData(1.10, false)]
    [Theory]
    public void MaxValueValidation(double? valueToValidate, bool isValidExpectedResult)
    {
        Assert.Equal(isValidExpectedResult, new MaximumValueAttribute(1.05).IsValid(valueToValidate));
    }

    [InlineData(null, false, false)]
    [InlineData(null, true, true)]
    [Theory]
    public void MaxValueAllowNullValidation(double? valueToValidate, bool allowNull, bool isValidExpectedResult)
    {
        Assert.Equal(isValidExpectedResult, new MaximumValueAttribute(0, allowNull).IsValid(valueToValidate));
    }
}
