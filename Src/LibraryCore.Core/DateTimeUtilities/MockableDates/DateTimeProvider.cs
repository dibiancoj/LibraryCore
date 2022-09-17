namespace LibraryCore.Core.DateTimeUtilities.MockableDates;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetNow() => DateTime.Now;
}
