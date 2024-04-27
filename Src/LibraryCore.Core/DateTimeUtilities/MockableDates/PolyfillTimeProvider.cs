using System.Diagnostics;

namespace System;

#if NET6_0 || NET7_0

//
// Summary:
//     Provides an abstraction for time.
public abstract class TimeProvider
{
    /// <summary>
    /// Gets a <see cref="TimeProvider"/> that provides a clock based on <see cref="DateTimeOffset.UtcNow"/>,
    /// a time zone based on <see cref="TimeZoneInfo.Local"/>, a high-performance time stamp based on <see cref="Stopwatch"/>,
    /// </summary>
    /// <remarks>
    /// If the <see cref="TimeZoneInfo.Local"/> changes after the object is returned, the change will be reflected in any subsequent operations that retrieve <see cref="GetLocalNow"/>.
    /// </remarks>
    public static TimeProvider System { get; } = new SystemTimeProvider();

    /// <summary>
    /// Initializes the <see cref="TimeProvider"/>.
    /// </summary>
    protected TimeProvider()
    {
    }

    /// <summary>
    /// Gets a <see cref="DateTimeOffset"/> value whose date and time are set to the current
    /// Coordinated Universal Time (UTC) date and time and whose offset is Zero,
    /// all according to this <see cref="TimeProvider"/>'s notion of time.
    /// </summary>
    /// <remarks>
    /// The default implementation returns <see cref="DateTimeOffset.UtcNow"/>.
    /// </remarks>
    public virtual DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;

    private static readonly long s_minDateTicks = DateTime.MinValue.Ticks;
    private static readonly long s_maxDateTicks = DateTime.MaxValue.Ticks;

    /// <summary>
    /// Gets a <see cref="DateTimeOffset"/> value that is set to the current date and time according to this <see cref="TimeProvider"/>'s
    /// notion of time based on <see cref="GetUtcNow"/>, with the offset set to the <see cref="LocalTimeZone"/>'s offset from Coordinated Universal Time (UTC).
    /// </summary>
    public DateTimeOffset GetLocalNow()
    {
        DateTimeOffset utcDateTime = GetUtcNow();
        TimeZoneInfo zoneInfo = LocalTimeZone;
        TimeSpan offset = zoneInfo.GetUtcOffset(utcDateTime);
        if (offset.Ticks is 0)
        {
            return utcDateTime;
        }

        long localTicks = utcDateTime.Ticks + offset.Ticks;
        if ((ulong)localTicks > (ulong)s_maxDateTicks)
        {
            localTicks = localTicks < s_minDateTicks ? s_minDateTicks : s_maxDateTicks;
        }

        return new DateTimeOffset(localTicks, offset);
    }

    /// <summary>
    /// Gets a <see cref="TimeZoneInfo"/> object that represents the local time zone according to this <see cref="TimeProvider"/>'s notion of time.
    /// </summary>
    /// <remarks>
    /// The default implementation returns <see cref="TimeZoneInfo.Local"/>.
    /// </remarks>
    public virtual TimeZoneInfo LocalTimeZone => TimeZoneInfo.Local;

    /// <summary>
    /// Gets the frequency of <see cref="GetTimestamp"/> of high-frequency value per second.
    /// </summary>
    /// <remarks>
    /// The default implementation returns <see cref="Stopwatch.Frequency"/>. For a given TimeProvider instance, the value must be idempotent and remain unchanged.
    /// </remarks>
    public virtual long TimestampFrequency => Stopwatch.Frequency;

    /// <summary>
    /// Gets the current high-frequency value designed to measure small time intervals with high accuracy in the timer mechanism.
    /// </summary>
    /// <returns>A long integer representing the high-frequency counter value of the underlying timer mechanism. </returns>
    /// <remarks>
    /// The default implementation returns <see cref="Stopwatch.GetTimestamp"/>.
    /// </remarks>
    public virtual long GetTimestamp() => Stopwatch.GetTimestamp();


    /// <summary>
    /// Used to create a <see cref="TimeProvider"/> instance returned from <see cref="System"/> and uses the default implementation
    /// </summary>
    private sealed class SystemTimeProvider : TimeProvider
    {
        /// <summary>Initializes the instance.</summary>
        internal SystemTimeProvider() : base()
        {
        }
    }
}

#endif