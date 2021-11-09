using System;

namespace LibraryCore.Core.ExtensionMethods;

public static class DateTimeExtensionMethods
{

    /// <summary>
    /// Is the specific date between the start and end date passed in
    /// </summary>
    /// <param name="dateTime">Date time to see if its after the start date and less then the end date</param>
    /// <param name="startDateTest">Start date to test</param>
    /// <param name="endDateTest">End date to test</param>
    /// <returns>True if the datetime is between the 2 dates</returns>
    public static bool IsBetween(this DateTime dateTime, DateTime startDateTest, DateTime endDateTest) => dateTime >= startDateTest && dateTime < endDateTest;

}
