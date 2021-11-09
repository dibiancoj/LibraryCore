using LibraryCore.Core.DateTimeUtilities;
using System;
using Xunit;

namespace LibraryCore.Tests.Core.DateTimeUtilities;

public class DateTimeUtilityTest
{

    #region Age

    [Fact]
    public void CalculateAgeTest1()
    {
        Assert.Equal(0, DateTimeUtility.Age(DateTime.Today.AddDays(-5)));
    }

    [Fact]
    public void CalculateAgeTest2()
    {
        Assert.Equal(1, DateTimeUtility.Age(DateTime.Today.AddYears(-1).AddDays(-1)));
    }

    [Fact]
    public void CalculateAgeTest3()
    {
        Assert.Equal(2, DateTimeUtility.Age(DateTime.Today.AddYears(-2)));
    }

    [Fact]
    public void CalculateAgeTest4()
    {
        Assert.Equal(2, DateTimeUtility.Age(DateTime.Today.AddYears(-2).AddDays(-3)));
    }

    #endregion

    #region Quarter

    /// <summary>
    /// Test which quarter this date is in
    /// </summary>
    [Fact]
    public void QuarterTimePeriodTest1()
    {
        Assert.Equal(1, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 1, 1)));
        Assert.Equal(1, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 2, 1)));
        Assert.Equal(1, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 3, 1)));

        Assert.Equal(2, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 4, 1)));
        Assert.Equal(2, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 5, 1)));
        Assert.Equal(2, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 6, 1)));

        Assert.Equal(3, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 7, 1)));
        Assert.Equal(3, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 8, 1)));
        Assert.Equal(3, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 9, 1)));

        Assert.Equal(4, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 10, 1)));
        Assert.Equal(4, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 11, 1)));
        Assert.Equal(4, DateTimeUtility.QuarterIsInTimePeriod(new DateTime(2014, 12, 1)));
    }

    #endregion

}
