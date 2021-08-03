using System;

namespace LibraryCore.Core.DateTimeUtilities
{
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
}
