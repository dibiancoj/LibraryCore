using System;
using System.Collections.Generic;
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
    public static IEnumerable<T> GetValuesLazy<T>() where T : Enum
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
        where T : struct
    {
        return Enum.TryParse<T>(valueToParse, out var tryToParseAttempt) ? tryToParseAttempt : null;
    }

    #endregion

    #region Get Custom Attribute Off Of Enum Member

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

}
