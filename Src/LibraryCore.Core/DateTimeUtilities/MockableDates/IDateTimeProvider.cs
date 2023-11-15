namespace LibraryCore.Core.DateTimeUtilities.MockableDates;

#if NET8_0_OR_GREATER
[Obsolete("Use TimeProvider In .net 8 Or C# 12")]
#endif
public interface IDateTimeProvider
{
    public DateTime GetNow();
}

