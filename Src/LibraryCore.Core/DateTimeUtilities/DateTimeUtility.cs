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
    }
}
