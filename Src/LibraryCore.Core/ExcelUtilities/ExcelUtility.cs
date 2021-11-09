using System;

namespace LibraryCore.Core.ExcelUtilities;

/// <summary>
/// Holds Any Excel Based Tools. Used When Creating Workbooks And Worksheets Programmically
/// </summary>
public static class ExcelUtility
{
    /// <summary>
    /// Converts The Column Such As A Or AA Into The Column Index
    /// </summary>
    /// <param name="columnLetter">Column Letter</param>
    /// <returns>Column Index</returns>
    /// <remarks>Hasn't been throughly tested</remarks>
    public static int ColumnLetterToColumnIndex(string columnLetter)
    {
        //Start with a base of 0
        int workingNumber = 0;

        //start with a power of 1
        int powerToUse = 1;

        //let's loop through the length until we have what we need
        for (int i = columnLetter.Length - 1; i >= 0; i--)
        {
            //add the column letter to the power
            workingNumber += (columnLetter[i] - 'A' + 1) * powerToUse;

            //now multiply by 26
            powerToUse *= 26;
        }

        //return the number now
        return workingNumber;
    }

    /// <summary>
    /// Convert A Column Number To Its Alpha Bet Letter. Used Mainly When Using For Loops. You Convert The Number Into The Column Letter When Setting The Range Value With A AlphaBet Character
    /// </summary>
    /// <param name="ColumnNumber">Int - Column Number</param>
    /// <returns>String - Alpha Bet Character Which Is The Equivalant To The Numeric Column Number Passed In</returns>
    public static string ColumnIndexToColumnLetter(int columnNumber)
    {
        //Holds the Alpha Base Value. Alpha Characters Start At 64
        const int alphaBase = 64;

        //Holds The Secondary Base Value. 
        const int secondBase = 26;

        // If 1-26, then this is an easy conversion. Its 1 letter.
        if (columnNumber < 27)
        {
            //return the character associated with the key...Add the base because alpha characters start at 64
            return Convert.ToString(Convert.ToChar((columnNumber + alphaBase)));
        }

        //Now we have to account for AA-ZZ

        //Set the first letter. Column number / base figure
        int firstLetter = (columnNumber / secondBase);

        //Set the second letter. (% = Mod [remainder after /])
        int secondLetter = (columnNumber % secondBase);

        //If the remainder is 0
        if (secondLetter == 0)
        {
            //If the remainder is 0 then it is Z, which should be 26
            secondLetter = secondBase;

            //you need to subtract 1 from the initial letter otherwise your lettering will be the next letter in the alphabet
            firstLetter--;
        }

        //return the value...first letter then second letter. Conver the values to a string before returning it...Need to convert both of the letter
        return Convert.ToString(Convert.ToChar((firstLetter + alphaBase))) +
               Convert.ToString(Convert.ToChar((secondLetter + alphaBase)));
    }
}
