﻿using LibraryCore.Core.DataTypes;
using LibraryCore.Core.Reflection;
using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LibraryCore.Shared;

namespace LibraryCore.Core.DataTableUtilities;

/// <summary>
/// Builds a data table from an object or a list of objects
/// </summary>
public static class ToDataTable
{

    #region Public Methods

    /// <summary>
    /// Builds a data table off of an object
    /// </summary>
    /// <typeparam name="T">Type of the object to build off of</typeparam>
    /// <param name="objectToBuildDataTableOffOf">object to build</param>
    /// <param name="tableName">Table Name. Property Off Of The Data Table</param>
    /// <returns>Data Table</returns>
    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    public static DataTable BuildDataTableFromObject<T>(T objectToBuildDataTableOffOf, string tableName)
        where T : class
    {
        //*** we need to have different method names because if we pass in a list<T> it will go into this overload. Because a list<T> is T.
        // then we get blow ups. we need to manually pass in which method to use

        //if this is a list...then blow it up and tell the user to use the other overload
        if (objectToBuildDataTableOffOf is IEnumerable)
        {
            throw new ArgumentOutOfRangeException(nameof(objectToBuildDataTableOffOf), "Please Use The BuildDataTableFromObjectList Overload When Passing In T Of IEnumerable");
        }

        //use the helper method...create an array of this object and pass it in
        return BuildDataTableFromListOfObjects(new[] { objectToBuildDataTableOffOf }, tableName);
    }

    /// <summary>
    /// Builds a data table off of the list of objects passed in
    /// </summary>
    /// <typeparam name="T">Type of the object to build off of</typeparam>
    /// <param name="objectsToBuildDataTableOffOf">List of objects to build</param>
    /// <param name="tableName">Table Name. Property Off Of The Data Table</param>
    /// <returns>Data table</returns>
    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    public static DataTable BuildDataTableFromListOfObjects<T>(IEnumerable<T> objectsToBuildDataTableOffOf, string tableName)
        where T : class
    {
        //*** we need to have different method names because if we pass in a list<T> it will go into this overload. Because a list<T> is T.
        // then we get blow ups. we need to manually pass in which method to use

        //create the data table to return
        var dataTableToBuild = new DataTable(tableName);

        //grab the properies we care about
        var propertiesToBuild = PropertiesToBuildOffOf<T>().ToArray();

        //*** with this version of the library. Just going to use reflection. If necessary the old version has expression trees which is faster ***

        //let's loop through the properties to build up the column def's 
        foreach (var propertyToBuild in propertiesToBuild)
        {
            //if its a nullable field then we need to 
            dataTableToBuild.Columns.Add(ReflectionUtility.IsNullableOfT(propertyToBuild) ?
                                             new DataColumn(propertyToBuild.Name, propertyToBuild.PropertyType.GetGenericArguments()[0]) { AllowDBNull = true } :
                                             new DataColumn(propertyToBuild.Name, propertyToBuild.PropertyType));
        }

        //now we need to go through each object and add the row
        foreach (T objectToBuildRowWith in objectsToBuildDataTableOffOf)
        {
            //let's create the new data row
            var newDataRow = dataTableToBuild.NewRow();

            //let's loop through all the properties and set the column
            foreach (var propertyToSet in propertiesToBuild)
            {
                //grab the value and set it...if its null we set the value to Db Null
                newDataRow[propertyToSet.Name] = propertyToSet.GetValue(objectToBuildRowWith) ?? DBNull.Value;
            }

            //let's add the data row to the data table
            dataTableToBuild.Rows.Add(newDataRow);
        }

        //return the data table
        return dataTableToBuild;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Build the properties to build the data table off of. Ignores collections because we can't model that in a data table
    /// </summary>
    /// <typeparam name="T">Type of the object to build</typeparam>
    /// <returns>list of property info to build off of</returns>
    [RequiresUnreferencedCode(ErrorMessages.AotDynamicAccess)]
#if NET7_0_OR_GREATER
    [RequiresDynamicCode(ErrorMessages.AotDynamicAccess)]
#endif
    private static IEnumerable<PropertyInfo> PropertiesToBuildOffOf<T>()
    {
        //grab just PrimitiveTypes we care about. (no collections or anything like that)
        var primativeTypesToBuild = PrimitiveTypes.PrimitiveTypesSelect();

        //grab just PrimitiveTypes we care about. No collections or anything like that and return the result
        return typeof(T).GetProperties().Where(x => primativeTypesToBuild.Contains(x.PropertyType));
    }

    #endregion

}

