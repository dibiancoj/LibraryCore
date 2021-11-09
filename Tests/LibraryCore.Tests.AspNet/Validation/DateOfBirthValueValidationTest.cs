using LibraryCore.AspNet.Validation;
using System;
using Xunit;

namespace LibraryCore.Tests.AspNet.Validation;

public class DateOfBirthValueValidationTest
{
    [InlineData(-300, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(-10, true)]
    [Theory]
    public void DateOfBirthValidation(int yearsToAddToTest, bool isValidExpectedResult)
    {
        Assert.Equal(isValidExpectedResult, new DateOfBirthRangeAttribute().IsValid(DateTime.Now.AddYears(yearsToAddToTest)));
    }
}
