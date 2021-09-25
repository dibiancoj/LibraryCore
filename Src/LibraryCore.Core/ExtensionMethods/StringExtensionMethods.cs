using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace LibraryCore.Core.ExtensionMethods
{
    public static class StringExtensionMethods
    {

        #region Contains

        /// <summary>
        /// Extension Method For A String To See If It Contains A String And Let The User Use String Comparison
        /// </summary>
        /// <param name="stringsToLookThrough">String List To Look For The Value In</param>
        /// <param name="valueToCheckTheStringFor">Value to check inside the string</param>
        /// <param name="whichComparison">Which comparison to use</param>
        /// <returns>Boolean if it contains that value</returns>
        /// <remarks>stringToCheckIn.Contains("ValueToCheckForInSide (stringToCheckIn)", StringComparison.OrdinalIgnoreCase);</remarks>
        public static bool Contains(this IEnumerable<string> stringsToLookThrough, string valueToCheckTheStringFor, StringComparison whichComparison)
        {
            //we could use an Any() call in linq but it was a tad bit slower. Since this is a hot path in most of my stuff going to leave it in the foreach loop
            foreach (var stringToTest in stringsToLookThrough)
            {
                //use the singlar method so we have code reuse
                if (stringToTest.Contains(valueToCheckTheStringFor, whichComparison))
                {
                    //we found a match, so return true
                    return true;
                }
            }

            //can't find the item
            return false;
        }

        #endregion

        #region String Is Null Or Empty - Instance

        /// <summary>
        /// Returns true if this string is neither null or empty
        /// </summary>
        /// <param name="stringToValidate">String to validate</param>
        /// <returns>Is the string has a value</returns>
        /// <remarks>Easier to write and flows more naturally then string.IsNullOrEmpty</remarks>
        public static bool HasValue([NotNullWhen(true)] this string? stringToValidate) => !string.IsNullOrEmpty(stringToValidate);

        /// <summary>
        /// Returns true if this string is either null or empty
        /// </summary>
        /// <param name="stringToValidate">String to validate</param>
        /// <returns>Is the string is either null or empty</returns>
        /// <remarks>Easier to write and flows more naturally then string.IsNullOrEmpty</remarks>
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? stringToValidate) => string.IsNullOrEmpty(stringToValidate);

        #endregion

        #region Is Null Or White Space

        /// <summary>
        /// Is null or white space on the string instance. This way you flows easier with the syntax
        /// </summary>
        /// <param name="stringToValidate">string to evaluate</param>
        /// <returns>if the string is null or whitespace</returns>
        /// <remarks>Easier to write and flows more naturally then string.IsNullOrWhiteSpace</remarks>
        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? stringToValidate) => string.IsNullOrWhiteSpace(stringToValidate);

        #endregion

        #region To Stream

        /// <summary>
        /// Write a string into a stream
        /// </summary>
        /// <param name="stringToWriteIntoAStream">String to write into a stream</param>
        /// <returns>The memory stream with the string written into it. Be Sure to Dispose of it!</returns>
        public static MemoryStream ToMemoryStream(this string stringToWriteIntoAStream)
        {
            //can't dispose of anything otherwise you won't be able to read it...The calling method needs to make sure they dispose of the stream

            //create the memory stream
            var memoryStreamToUse = new MemoryStream();

            //create the writer
            var writerToUse = new StreamWriter(memoryStreamToUse);

            //write the string data
            writerToUse.Write(stringToWriteIntoAStream);

            //flush it out
            writerToUse.Flush();

            //set the position to the beg of the stream
            memoryStreamToUse.Position = 0;

            //go run the test method
            return memoryStreamToUse;
        }

        #endregion

        #region To Byte Array - Utf 8

        /// <summary>
        /// Converts a string to a byte array using utf 8
        /// </summary>
        /// <param name="stringToConvertToByteArray">String to convert</param>
        /// <returns>byte array</returns>
        /// <remarks>Used in the past, if i have a string which is sql, i convert it to a file (byte array), then send it for download on the web server without saving it to a temporary file</remarks>
        public static byte[] ToByteArray(this string stringToConvertToByteArray) => Encoding.UTF8.GetBytes(stringToConvertToByteArray);

        #endregion

        #region Surround With

        /// <summary>
        /// Surround a string with quotes
        /// </summary>
        /// <param name="stringToQuote">String to put quotes around</param>
        /// <returns>String with the value at the beg and end of it</returns>
        public static string SurroundWithQuotes(this string stringToQuote) => stringToQuote.SurroundWith("\"");

        /// <summary>
        /// Surround a string with a specific character at the front and back
        /// </summary>
        /// <param name="stringToQuote">String to put characters around it. Front and Back of the string</param>
        /// <param name="stringToAddAtBegAndEnd">String value to add at the beg and end of the passed in string</param>
        /// <returns>String with the value at the beg and end of it</returns>
        /// <remarks>benchmarkdot net showed that a string builder or ${}... was slower and used more memory then just appending 3 items</remarks>
        public static string SurroundWith(this string stringToQuote, string stringToAddAtBegAndEnd) => $"{stringToAddAtBegAndEnd}{stringToQuote}{stringToAddAtBegAndEnd}";

        #endregion

        #region Base 64 Encoding

        /// <summary>
        /// Convert a string to a base 64 encoded string. Can be used for basic authentication
        /// </summary>
        /// <param name="stringToEncode">string to convert to base 64 encoded</param>
        /// <returns>Encoded base 64 string</returns>
        public static string ToBase64Encode(this string stringToEncode) => Convert.ToBase64String(Encoding.UTF8.GetBytes(stringToEncode));

        /// <summary>
        /// Convert a base 64 encoded string back to a regular string value
        /// </summary>
        /// <param name="stringToEncode">string to decode from base 64 encoded</param>
        /// <returns>Encoded base 64 string</returns>
        public static string ToBase64Decode(this string stringToEncode) => Encoding.UTF8.GetString(Convert.FromBase64String(stringToEncode));

        #endregion

        #region Format Zip Code

        /// <summary>
        /// Format a string to a USA Zip Code
        /// </summary>
        /// <param name="zipCode">Zip Code - 9 characters</param>
        /// <returns>Outputted with - then the rest of the 4 extension zip #'s</returns>
        /// <remarks>Needs to use this instead of string.format because it can't handle a leading 0</remarks>
        public static string? ToUSAZipCode(this string? zipCode)
        {
            //if the zip code is null then just return it right away
            if (string.IsNullOrEmpty(zipCode))
            {
                //zip code is null / blank...just return the string that was passed in
                return zipCode;
            }

            //grab just the digits
            var justDigitsInSpan = zipCode.PullDigitsFromString().AsSpan();

            if (justDigitsInSpan.Length == 5)
            {
                //if its just the 5 character version, then return just the item passed in
                return zipCode;
            }

            if (justDigitsInSpan.Length != 9)
            {
                //if the length is not 9 then return it...we need 9 characters
                return zipCode;
            }

            //we have 9 characters, create the instance of the string builder becauase we need it (init the capacity to reduce memory just a tag)
            return new StringBuilder()
                    .Append(justDigitsInSpan.Slice(0, 5))
                    .Append('-')
                    .Append(justDigitsInSpan.Slice(5, 4))
                    .ToString();
        }

        #endregion

        #region Format USA Phone Number

        /// <summary>
        /// Format a string to a USA Phone Number
        /// </summary>
        /// <param name="phoneNumber">Phone Number To Format - Needs to be 10 characters otherwise will just return the number</param>
        /// <returns>Outputted with ( and -</returns>
        /// <remarks>Needs to use this instead of string.format because it can't handle a leading 0</remarks>
        public static string? ToUSAPhoneNumber(this string? phoneNumber)
        {
            //make sure the phone is not null Or the length is 10 characters
            if (string.IsNullOrEmpty(phoneNumber))
            {
                //not 10 digits, just return whatever was passed in
                return phoneNumber;
            }

            //clense it and just grab all the digits
            var phoneNumberJustDigits = phoneNumber.PullDigitsFromString().AsSpan();

            //is not 10 digits?
            if (phoneNumberJustDigits.Length != 10)
            {
                return phoneNumber;
            }

            //we need the string builder...so create the object (init the capacity in the sb to reduce memory a tad)
            return new StringBuilder()
                //set the area code
                .Append('(')
                .Append(phoneNumberJustDigits.Slice(0, 3))
                .Append(") ")

                //now lets set the first 3 digits of the regular #
                .Append(phoneNumberJustDigits.Slice(3, 3))

                //add the dash
                .Append('-')

                //add the last 4
                .Append(phoneNumberJustDigits.Slice(6, 4))

                .ToString();
        }

        #endregion

        #region All Indexes Of Character

        public static IEnumerable<int> AllIndexes(this string stringToLookThrough, char characterToLookFor)
        {
            int minIndex = stringToLookThrough.IndexOf(characterToLookFor);

            while (minIndex != -1)
            {
                yield return minIndex;

                minIndex = stringToLookThrough.IndexOf(characterToLookFor, minIndex + 1);
            }
        }

        #endregion

        #region Digits

        public static int NumberOfDigitsInTheString(this string stringToLookThrough)
        {
            //faster then regex, string reader, etc. Leaving this for speed and lean memory usage(HowManyDigitsTest.cs in perf project)
            int i = 0;

            foreach (var characterToTest in stringToLookThrough)
            {
                if (char.IsDigit(characterToTest))
                {
                    i++;
                }
            }

            return i;
        }

        /// <summary>
        /// Grabs all the digits in the string and returns a span
        /// </summary>
        /// <returns>string with only the digits</returns>
        public static string PullDigitsFromString(this string stringToLookThrough)
        {
            //this is here for performance reasons. Much faster and uses less memory then new string(phoneNumber.Where(char.IsDigit).ToArray()).AsSpan(); --> StringNumberToStringTest.cs in the perf project
            var builder = new StringBuilder();

            foreach (var characterToTest in stringToLookThrough)
            {
                if (char.IsDigit(characterToTest))
                {
                    builder.Append(characterToTest);
                }
            }

            return builder.ToString();
        }

        #endregion

        #region Replace All Tags

        /// <summary>
        /// Look through rawTextToSearchAndReplace and replace any instances of the keys in tagsToReplace and replace them with the value
        /// </summary>
        /// <param name="rawTextToSearchAndReplace">The string to look and do the replacement with</param>
        /// <param name="tagsToReplace">The lookup where you search for these phrases (key) and replace with (value)</param>
        /// <returns>The updated string</returns>
        public static string ReplaceAllTagsInString(this string rawTextToSearchAndReplace, IEnumerable<KeyValuePair<string, string>> tagsToReplace)
        {
            var temp = new StringBuilder(rawTextToSearchAndReplace);

            foreach (var tagToReplace in tagsToReplace.Where(x => rawTextToSearchAndReplace.Contains(x.Key)))
            {
                temp.Replace(tagToReplace.Key, tagToReplace.Value);
            }

            return temp.ToString();
        }

        #endregion

    }
}
