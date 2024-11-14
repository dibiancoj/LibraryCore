namespace LibraryCore.Core.DateTimeUtilities.BusinessDays;

public static class BusinessDayCalculations
{
    public static int NumberOfBusinessDaysBetweenDates(DateTime startDate, DateTime endDate) => NumberOfBusinessDaysBetweenDates(startDate, endDate, []);

    public static int NumberOfBusinessDaysBetweenDates(DateTime startDate, DateTime endDate, IEnumerable<DateTime> holidaysToExclude) => DaysBetween2Dates(startDate, endDate, holidaysToExclude).Count();

    public static IEnumerable<DateTime> DaysBetween2Dates(DateTime startDate, DateTime endDate, IEnumerable<DateTime> holidaysToExclude)
    {
        var workingDate = startDate.Date;

        while (workingDate < endDate.Date)
        {
            if (workingDate.DayOfWeek != DayOfWeek.Saturday && workingDate.DayOfWeek != DayOfWeek.Sunday && !holidaysToExclude.Contains(workingDate))
            {
                yield return workingDate;
            }

            //increase the date
            workingDate = workingDate.AddDays(1);
        }
    }
}
