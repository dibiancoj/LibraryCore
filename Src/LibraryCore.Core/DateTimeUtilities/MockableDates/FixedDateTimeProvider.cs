namespace LibraryCore.Core.DateTimeUtilities.MockableDates;

/// <summary>
/// Provider where we can mock dates. This provider is great for unit tests
/// </summary>
#if NET8_0_OR_GREATER
[Obsolete("Use TimeProvider In .net 8 Or C# 12")]
#endif
public class FixedDateTimeProvider(DateTime fixedDate) : IDateTimeProvider
{
    private DateTime FixedDate { get; } = fixedDate;

    public DateTime GetNow() => FixedDate;
}
