namespace LibraryCore.Core.DateTimeUtilities;

public static class DateTimeUtility
{
    /// <summary>
    /// Get the age of a person, date, etc.
    /// </summary>
    /// <param name="dateOfBirth">birth date</param>
    /// <returns>Age in years</returns>
    public static int Age(DateTime dateOfBirth)
    {
        //grab the date today
        var today = DateTime.Today;

        //calculate the age
        var age = today.Year - dateOfBirth.Year;

        //go back to the year if the person was born on a leap year
        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        //return the age
        return age;
    }

    /// <summary>
    /// Days until next bday
    /// </summary>
    /// <param name="dateTimeProvider">Date time provider. Call TimeProvider.System to pass in for normal runtime code</param>
    /// <param name="dateOfBirth">Date of birth</param>
    /// <returns>Days until next bday</returns>
    public static int DaysUntilNextBday(TimeProvider dateTimeProvider, DateTime dateOfBirth)
    {
        var today = dateTimeProvider.GetLocalNow().Date;
        var nextBday = new DateTime(today.Year, dateOfBirth.Month, dateOfBirth.Day);

        if (nextBday < today)
        {
            nextBday = nextBday.AddYears(1);
        }

        return (nextBday - today).Days;
    }

    /// <summary>
    /// Figure out which quarter this time period falls in
    /// </summary>
    /// <param name="whichQuarterIsDateTimeIn">Date time to figure out which quarter this falls in</param>
    /// <returns>Which Quarter 1 through 4</returns>
    public static int QuarterIsInTimePeriod(DateTime whichQuarterIsDateTimeIn)
    {
        //determine which quarter by the month
        return whichQuarterIsDateTimeIn.Month switch
        {
            1 or 2 or 3 => 1,
            4 or 5 or 6 => 2,
            7 or 8 or 9 => 3,
            10 or 11 or 12 => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(whichQuarterIsDateTimeIn), $"Invalid Month Of ${whichQuarterIsDateTimeIn.Month}"),
        };
    }
}
