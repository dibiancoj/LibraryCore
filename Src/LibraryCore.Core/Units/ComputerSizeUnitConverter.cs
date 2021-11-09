using System;

namespace LibraryCore.Core.Units;

/// <summary>
/// Helps convert computer units from one to another. For example convert 2 bytes to kilobytes
/// </summary>
public static class ComputerSizeUnitConverter
{

    #region Enum

    /// <summary>
    /// Unit Enum
    /// </summary>
    public enum ComputerSizeUnit : int
    {

        /// <summary>
        /// Bytes 
        /// </summary>
        Byte = 1,

        /// <summary>
        /// KiloBytes (KB)
        /// </summary>
        Kilobyte = 2,

        /// <summary>
        /// MegaBytes (MB)
        /// </summary>
        Megabyte = 3,

        /// <summary>
        /// GigaBytes (GB)
        /// </summary>
        Gigabyte = 4,

        /// <summary>
        /// Terabytes (TB)
        /// </summary>
        Terabyte = 5

    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Converts the From Value From The FromUnit to the ToUnit
    /// </summary>
    /// <param name="fromUnit">From Unit To Convert From</param>
    /// <param name="toUnit">The Unit To Convert Too</param>
    /// <param name="valueToConvert">Value To Convert </param>
    /// <returns>Converted Value</returns>
    /// <remarks>Used this web site to verify the calcuations http://www.unit-conversion.info/computer.html</remarks>
    public static double ConvertUnitCalcuation(ComputerSizeUnit fromUnit, ComputerSizeUnit toUnit, double valueToConvert)
    {
        //The base for computers is 1024
        const int computerBase = 1024;

        //how the enum works. basically everything starts with bytes (level 1)
        //level 2 is the next increment...so we use the enum's int value to determine
        //how many times we need to loop through and keep adding to the figure

        //how many levels ^ computer base...so every level we either * by 1024...or / by 1024. (start with 1 if we are going from the same from and to level)
        double LevelsPerComputerBase = 1;

        //are we going to different levels?
        if (fromUnit != toUnit)
        {
            //we are going multiple levels, so we need to calculate how many levels we are going (we can go higher or lower)

            //so we get the # of units we are going. Then grab the power...so we are going 2 levels and then do (1024 ^ 2 levels)
            LevelsPerComputerBase = Math.Pow(computerBase, Math.Abs(fromUnit - toUnit));
        }

        //if we are going smaller to lower then we need to divide the LevelsPerComputerBase by / 
        if (fromUnit < toUnit)
        {
            //we need to divide by 1024 to get to the smaller unit
            return valueToConvert / LevelsPerComputerBase;
        }

        //let's multiple the value we want to convert to the working figure...(at this point we have the multipler from the base unit to the ToUnit)
        return valueToConvert * LevelsPerComputerBase;
    }

    #endregion

}
