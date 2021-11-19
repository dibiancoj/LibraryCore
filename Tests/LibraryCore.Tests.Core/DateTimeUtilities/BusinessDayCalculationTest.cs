using LibraryCore.Core.DateTimeUtilities.BusinessDays;
using Xunit;

namespace LibraryCore.Tests.Core.DateTimeUtilities;

public class BusinessDayCalculationTest
{

    #region No Holidays

    [Fact]
    public void SameDay() => Assert.Equal(0, BusinessDayCalculations.NumberOfBusinessDaysBetweenDates(new DateTime(2021, 9, 20), new DateTime(2021, 9, 20)));

    [Fact]
    public void OneDay() => Assert.Equal(1, BusinessDayCalculations.NumberOfBusinessDaysBetweenDates(new DateTime(2021, 9, 20), new DateTime(2021, 9, 21)));

    [Fact]
    public void TwoDays() => Assert.Equal(2, BusinessDayCalculations.NumberOfBusinessDaysBetweenDates(new DateTime(2021, 9, 20), new DateTime(2021, 9, 22)));

    [Fact]
    public void FridayToTuesdayWithWeekends() => Assert.Equal(2, BusinessDayCalculations.NumberOfBusinessDaysBetweenDates(new DateTime(2021, 9, 24), new DateTime(2021, 9, 28)));

    [Fact]
    public void TwoWeekendsInTheMix() => Assert.Equal(8, BusinessDayCalculations.NumberOfBusinessDaysBetweenDates(new DateTime(2021, 9, 24), new DateTime(2021, 10, 6)));

    #endregion

    #region Holidays In The Mix

    [Fact]
    public void OneHolidayInTheMix() => Assert.Equal(1, BusinessDayCalculations.NumberOfBusinessDaysBetweenDates(new DateTime(2021, 9, 24), new DateTime(2021, 9, 28), new DateTime[] { new DateTime(2021, 9, 24) }));

    [Fact]
    public void TwoHolidayInTheMix() => Assert.Equal(6, BusinessDayCalculations.NumberOfBusinessDaysBetweenDates(new DateTime(2021, 9, 24), new DateTime(2021, 10, 6), new DateTime[] { new DateTime(2021, 9, 24), new DateTime(2021, 10, 5) }));

    #endregion

}
