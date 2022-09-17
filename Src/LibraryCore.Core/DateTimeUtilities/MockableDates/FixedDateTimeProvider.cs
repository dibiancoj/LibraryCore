namespace LibraryCore.Core.DateTimeUtilities.MockableDates;

/// <summary>
/// Provider where we can mock dates. This provider is great for unit tests
/// </summary>
public class FixedDateTimeProvider : IDateTimeProvider
{
    public FixedDateTimeProvider(DateTime fixedDate)
    {
        FixedDate = fixedDate;
    }

    private DateTime FixedDate { get; }

    public DateTime GetNow() => FixedDate;
}
