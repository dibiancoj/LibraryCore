using LibraryCore.AspNet.Validation;
using Xunit;

namespace LibraryCore.Tests.AspNet.Validation
{
    public class MinimumValueValidationTest
    {
        [InlineData(null, false)] //default value
        [InlineData(0, false)]
        [InlineData(.01, true)]
        [InlineData(1.25, true)]
        [InlineData(1.5, true)]
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
}
