using LibraryCore.AspNet.Validation;

namespace LibraryCore.Tests.AspNet.Validation;

public class DateOfBirthValueValidationTest
{
    [InlineData(-126, false, true)]
    [InlineData(0, true, true)]
    [InlineData(1, false, true)]
    [InlineData(2, false, true)]
    [InlineData(-10, true, true)]
    [InlineData(1, false, false)]
    [InlineData(0, true, false)]
    [Theory]
    public void DateOfBirthValidation(int yearsToAddToTest, bool isValidExpectedResult, bool addYears)
    {
        var dateToValidate = addYears ? 
                                DateTime.Now.AddYears(yearsToAddToTest) : 
                                DateTime.Now.AddDays(yearsToAddToTest);

        Assert.Equal(isValidExpectedResult, new DateOfBirthRangeAttribute().IsValid(dateToValidate));
    }
}
