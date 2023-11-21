namespace LibraryCore.Core.ExtensionMethods;
public static class GuidExtensionMethods
{
    public static Guid SequentialGuidCreate(this Guid source)
    {
        byte[] guidArray = source.ToByteArray();

        var baseDate = new DateTime(1900, 1, 1);
        var now = DateTime.Now;

        // Get the days and milliseconds which will be used to build the byte string
        var days = new TimeSpan(now.Ticks - baseDate.Ticks);
        var msecs = now.TimeOfDay;

        // Convert to a byte array

        // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333
        byte[] daysArray = BitConverter.GetBytes(days.Days);
        byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

        // Reverse the bytes to match SQL Servers ordering
        Array.Reverse(daysArray);
        Array.Reverse(msecsArray);

        // Copy the bytes into the guid
        Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
        Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

        return new Guid(guidArray);
    }
}
