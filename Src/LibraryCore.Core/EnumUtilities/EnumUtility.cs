using System.Collections.Immutable;
using System.ComponentModel;
using System.Reflection;

namespace LibraryCore.Core.EnumUtilities;

/// <summary>
/// Utilities for enum's
/// </summary>
public static class EnumUtility
{

    #region Get Values

    /// <summary>
    /// Retrieve all the members of an enum
    /// </summary>
    /// <typeparam name="T">Type Of The Enum</typeparam>
    /// <returns>IEnumerable of your enum values</returns>
    public static IEnumerable<T> GetValuesLazy<T>() where T : struct, Enum
    {
        //loop through the types
        foreach (var enumType in Enum.GetValues(typeof(T)))
        {
            //return this value
            yield return (T)enumType!;
        }
    }

    #endregion

    #region Try Parse To Nullable

    /// <summary>
    /// Try to parse an enum from a string. This will return a nullable enum set to null if the value can't be parsed
    /// </summary>
    /// <typeparam name="T">Type of the enum</typeparam>
    /// <param name="valueToParse">value to try to parse to this type</param>
    /// <returns>null if can't be parsed. Otherwise will return the parsed enum value</returns>
    public static T? TryParseToNullable<T>(string valueToParse)
        where T : struct, Enum
    {
        return Enum.TryParse<T>(valueToParse, out var tryToParseAttempt) ? tryToParseAttempt : null;
    }

    #endregion

    #region Get Custom Attribute Off Of Enum Member

    /// <summary>
    /// Get a dictionary where the key is the enum value and the description is a DescriptionAttribute. This a common pattern that can be abstracted to just this method call
    /// </summary>
    /// <typeparam name="T">Type of the enum</typeparam>
    /// <returns>dictionary where the key is the enum value and the description is a DescriptionAttribute string</returns>
    public static IImmutableDictionary<T, string> EnumLookupTable<T>() where T : struct, Enum
    {
        return GetValuesLazy<T>().ToImmutableDictionary(x => x, x => CustomAttributeGet<DescriptionAttribute>(x).Description);
    }

    public static T? CustomAttributeTryGet<T>(Enum enumValueToRetrieve) where T : Attribute
    {
        //System.ComponentModel.DescriptionAttribute
        //[TestAttribute("Equals This Number")]
        //Equals = 1

        //grab the custom attributes now 
        return EnumFieldValueGet(enumValueToRetrieve).GetCustomAttribute<T>();
    }

    /// <summary>
    /// Method will retrieve a customm attribute off of the enum passed in.
    /// the items in your enum.
    /// </summary>
    /// <typeparam name="T">Custom Attribute Type To Look For</typeparam>
    /// <param name="enumValueToRetrieve">Enum Value To Retrieve The Attribute Off Of</param>
    /// <returns>Description Attribute Value Or Null If The Description Tag Is Not Found</returns>
    /// <remarks>Custom Attribute If Found. Otherwise Null</remarks>
    public static T CustomAttributeGet<T>(Enum enumValueToRetrieve) where T : Attribute
    {
        //System.ComponentModel.DescriptionAttribute
        //[TestAttribute("Equals This Number")]
        //Equals = 1

        //grab the custom attributes now 
        return CustomAttributeTryGet<T>(enumValueToRetrieve) ?? throw new NullReferenceException("Custom Attribute Of T - " + typeof(T).Name + " was not found");
    }

    /// <summary>
    /// Determine if an attribute value is present on the enum value
    /// </summary>
    /// <typeparam name="T">Attribute type to look for</typeparam>
    /// <param name="enumValueToTest">enum value to see if the attribute is defined on</param>
    /// <returns>True if the attribute is present. False is not</returns>
    public static bool AttributeIsDefined<T>(Enum enumValueToTest) where T : Attribute
    {
        return EnumFieldValueGet(enumValueToTest).IsDefined(typeof(T), false);
    }

    /// <summary>
    /// Returns the field info for the given enum value.
    /// </summary>
    /// <param name="enumValueToFetchFieldInfoFor">Enum value to get the field info for</param>
    /// <returns>FieldInfo for the enum passed in</returns>
    private static FieldInfo EnumFieldValueGet(Enum enumValueToFetchFieldInfoFor)
    {
        //grab the type of the enum passed in, then grab the field info object from the enum value passed in
        //this should never really return a null value because we passed in an enum. Can't think of a valid scenario here to retur null (ignoring nulls)
        return enumValueToFetchFieldInfoFor.GetType().GetField(enumValueToFetchFieldInfoFor.ToString())!;
    }

    #endregion

    #region Bit Mask

    //example of how to create the bit mask enum
    //[Flags]
    //public enum Days : int
    //{
    //    None = 0,
    //    Sunday = 1,
    //    Monday = 2,
    //    Tuesday = 4,
    //    Wednesday = 8,
    //    Thursday = 16,
    //    Friday = 32,
    //    Saturday = 64
    //}

    /// <summary>
    /// Takes a bit mask and add's to it and returns the updated enum value
    /// </summary>
    /// <typeparam name="T">Type of the enum</typeparam>
    /// <param name="workingEnumValue">The working enum. So the combination of all the enums that have been joined together</param>
    /// <param name="valueToAdd">value to add to the working enum value</param>
    /// <returns>the new updated enum that the value to add and the working enum value have been merged into</returns>
    public static T BitMaskAddItem<T>(T workingEnumValue, T valueToAdd) where T : struct, Enum
    {
        //add the logical or's together then parse it and return it
        return Enum.Parse<T>((Convert.ToInt64(workingEnumValue) | Convert.ToInt64(valueToAdd)).ToString());
    }

    /// <summary>
    /// Remove a value from a bit mask value
    /// </summary>
    /// <typeparam name="T">Type of the enum</typeparam>
    /// <param name="workingEnumValue">The working enum. So the combination of all the enums that have been joined together</param>
    /// <param name="valueToRemove">Value to remove</param>
    /// <returns>The updated enum value</returns>
    public static T BitMaskRemoveItem<T>(T workingEnumValue, T valueToRemove) where T : struct, Enum
    {
        return Enum.Parse<T>((Convert.ToInt64(workingEnumValue) & ~Convert.ToInt64(valueToRemove)).ToString());
    }

    /// <summary>
    /// Check to see if the value to check for (bit mask) is in the working enum value. ie is part of the bit mask.
    /// </summary>
    /// <typeparam name="T">Value of the enum</typeparam>
    /// <param name="workingEnumValue">Working Enum Value To Look In For The ValueToCheckFor</param>
    /// <param name="valueToCheckFor">Value To Check For In The Enum</param>
    /// <returns>True if it is in the enum. Ie is selected</returns>
    public static bool BitMaskContainsValue<T>(T workingEnumValue, T valueToCheckFor) where T : struct, Enum => BitMaskContainsValueHelper(workingEnumValue, valueToCheckFor);

    /// <summary>
    /// Returns all the selected flags in the working enum value.
    /// </summary>
    /// <typeparam name="T">Type of the enum</typeparam>
    /// <param name="workingEnumValue">Working enum value to look in</param>
    /// <returns>list of flags that are selected. Uses yield to chain. Use ToArray() to send the values to an array</returns>
    public static IEnumerable<T> BitMaskSelectedItems<T>(T workingEnumValue) where T : struct, Enum => GetValuesLazy<T>().Where(x => BitMaskContainsValueHelper(workingEnumValue, x));

    /// <summary>
    /// Check to see if the value to check for (bit mask) is in the working enum value. ie is part of the bit mask.
    /// Helper method is used so we don't have to validate the enum each time when called inside a loop such as BitMaskSelectedItems
    /// </summary>
    /// <typeparam name="T">Value of the enum</typeparam>
    /// <param name="workingEnumValue">Working Enum Value To Look In For The ValueToCheckFor</param>
    /// <param name="valueToCheckFor">Value To Check For In The Enum</param>
    /// <returns>True if it is in the enum. Ie is selected</returns>
    private static bool BitMaskContainsValueHelper<T>(T workingEnumValue, T valueToCheckFor) where T : struct, Enum => (Convert.ToInt64(workingEnumValue) & Convert.ToInt64(valueToCheckFor)) == Convert.ToInt64(valueToCheckFor);

    #endregion

}
