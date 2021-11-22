namespace LibraryCore.Core.DataTypes;

/// <summary>
/// Gives you the base types. The PropertyInfo.PropertyType.IsPrimitive doesnt give you string and the nullable types. This class will give you all the primitive types include nullable values
/// </summary>
public static class PrimitiveTypes
{
    /// <summary>
    /// Gives you the base types. The PropertyInfo.PropertyType.IsPrimitive doesnt give you string and the nullable types. This class will give you all the primitive types include nullable values
    /// </summary>
    /// <returns>List Of Types</returns>
    public static ISet<Type> PrimitiveTypesSelect() =>
        new HashSet<Type>(new Type[] {
                typeof(string),
                typeof(bool),
                typeof(bool?),
                typeof(DateOnly),
                typeof(DateOnly?),
                typeof(TimeOnly),
                typeof(TimeOnly?),
                typeof(DateTime),
                typeof(DateTime?),
                typeof(short),
                typeof(short?),
                typeof(int),
                typeof(int?),
                typeof(long),
                typeof(long?),
                typeof(double),
                typeof(double?),
                typeof(float),
                typeof(float?),
                typeof(decimal),
                typeof(decimal?) });

}
