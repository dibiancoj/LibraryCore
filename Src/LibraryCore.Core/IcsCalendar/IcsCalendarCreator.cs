using System.Text;
using static LibraryCore.Core.IcsCalendar.TimeZones.BaseTimeZoneFactory;

namespace LibraryCore.Core.IcsCalendar;

public static class IcsCalendarCreator
{

    /// <summary>
    /// Mime type for the ics file
    /// </summary>
    public const string ICSMimeType = "text/calendar";

    /// <summary>
    /// Format to use when outputting a date
    /// </summary>
    /// <remarks>Unit test uses reflection to grab these fields so we can validate the data</remarks>
    private const string FormatSpecificDateTime = "yyyyMMddTHHmmss";

    /// <summary>
    /// Format the date when it's a full day appt
    /// </summary>
    private const string FormatSpecificDateTimeForFullDayAppointment = "yyyyMMdd";

    public enum IcsTimeZoneEnum
    {
        NewYork
    }

    /// <summary>
    /// Creates an .ics file which creates an event in outlook, google calendar, etc.
    /// </summary>
    /// <param name="icsTimeZone">Time zone of appointment</param>
    /// <param name="startDateTimeOfAppointment">Start date or time of appointment</param>
    /// <param name="endDateTimeOfAppointment">End date or time of appointment</param>
    /// <param name="summaryOfAppointment">Summary description of the appointment</param>
    /// <param name="locationOfAppointment">Location of the appointment</param>
    /// <param name="bodyOfReminder">The body text of the reminder</param>
    /// <returns>A String. Either call  System.IO.File.WriteAllText("test.ics", result) to write it to disk. Or Encoding.ASCII.GetBytes(result) to get it into a byte array for download</returns>
    public static string CreateICSAppointment(IcsTimeZoneEnum icsTimeZone,
                                              DateTime startDateTimeOfAppointment,
                                              DateTime endDateTimeOfAppointment,
                                              string summaryOfAppointment,
                                              string locationOfAppointment,
                                              string bodyOfReminder,
                                              bool isFullDayAppointment)
    {
        /*Syntax should be something like this
         *BEGIN:VCALENDAR
         *VERSION:2.0
         *PRODID:-//hacksw/handcal//NONSGML v1.0//EN
         * {{the time zone definition data}}
         *BEGIN:VEVENT
         *DTSTART;TZID=America/New_York:20140606T180000
         *DTEND:;TZID=America/New_York:20140606T180000
         *SUMMARY: bla bla
         *LOCATION: New York
         *END:VEVENT
         *END:VCALENDAR
         *
         * For Dates:
         * If for specific date time [using start as an example]:
         *DTSTART:FormattedDateTimeYouWant
         *
         * If for entire day [using start as an example]
         *DTSTART;VALUE=DATE:FormattedDateTimeYouWant
         */

        //For line breaks use "\\n"

        //grab the time zone factory
        var timeZoneFactoryToUse = CreateTimeZone(icsTimeZone);

        //we will use a string builder to write everything. use a default capacity. 
        //there are basically 5 fields tht we need to fill in...start date, end date, summary, location, and the body.
        var icsWriter = new StringBuilder()
            .AppendLine("BEGIN:VCALENDAR")
            .AppendLine("VERSION:2.0")
            .AppendLine("PRODID:-//hacksw/handcal//NONSGML v1.0//EN")
            .AppendLine(timeZoneFactoryToUse.TimeZoneDefinitionOutput)
            .AppendLine("BEGIN:VEVENT");

        //We basically need for specific times
        //"DTSTART:Date:TheFormattedDateNow"
        //"DTEND:Date:TheFormattedDateNow"

        //else if full day appointment
        //"DTSTART;Value=Date:TheFormattedDateNow"
        //"DTEND;Value=Date:TheFormattedDateNow"

        if (isFullDayAppointment)
        {
            icsWriter
                .Append($"DTSTART;VALUE=DATE:{startDateTimeOfAppointment.ToString(FormatSpecificDateTimeForFullDayAppointment)}")
                .Append(Environment.NewLine)
                .Append($"DTEND;VALUE=DATE:{endDateTimeOfAppointment.ToString(FormatSpecificDateTimeForFullDayAppointment)}")
                .Append(Environment.NewLine);
        }
        else
        {
            icsWriter     
                .Append($"DTSTART;TZID={timeZoneFactoryToUse.TimeZoneDateOutput}:{GetFormattedDateTime(startDateTimeOfAppointment)}").Append(Environment.NewLine)
                .Append($"DTEND;TZID={timeZoneFactoryToUse.TimeZoneDateOutput}:{GetFormattedDateTime(endDateTimeOfAppointment)}").Append(Environment.NewLine);
        }

        //add the summary and return
        return icsWriter
                 .Append($"SUMMARY:{summaryOfAppointment}").Append(Environment.NewLine)
                 .Append($"LOCATION:{locationOfAppointment}").Append(Environment.NewLine)
                 .Append($"DESCRIPTION:{bodyOfReminder}").Append(Environment.NewLine)
                 .AppendLine("END:VEVENT")
                 .AppendLine("END:VCALENDAR")
                 .ToString();
    }

    /// <summary>
    /// Return the formatted time that an ics file expects.
    /// </summary>
    /// <param name="dateToBuild">Date to build up</param>
    /// <returns>the formatted time in a string</returns>
    private static string GetFormattedDateTime(DateTime dateToBuild) => dateToBuild.ToString(FormatSpecificDateTime);

}
