using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LibraryCore.Core.Reflection;

public static class ReflectionUtility
{

    #region Scanning For Types

    /// <summary>
    /// Scans assemblies to find all instances of TInterface ie: register all classes that implement IRepository. Use this overload for a normal app
    /// </summary>
    /// <typeparam name="TInterface">Interface type for the types that you want to register</typeparam>
    /// <returns>All the types that implement that interface</returns>
    [ExcludeFromCodeCoverage(Justification = "Not valid in a unit test context.")]
    public static IEnumerable<TypeInfo> ScanForAllInstancesOfType<TInterface>() => ScanForAllInstancesOfType<TInterface>(Assembly.GetEntryAssembly() ?? throw new Exception("No Entry Assembly"));

    /// <summary>
    /// Scans assemblies to find all instances of TInterface ie: register all classes that implement IRepository
    /// </summary>
    /// <typeparam name="TInterface">Interface type for the types that you want to register</typeparam>
    /// <param name="rootAssembly">Root rootAssembly. This is mainly used for unit testing where we don't have a real root. Use the other overload in a asp.net core app</param>
    /// <returns>All the types that implement that interface</returns>
    public static IEnumerable<TypeInfo> ScanForAllInstancesOfType<TInterface>(Assembly rootAssembly)
    {
        //get all the references Assembies
        var referencesAssemblies = rootAssembly.GetReferencedAssemblies()
                                    .Select(Assembly.Load);

        //NOT TESTABLE - No App Domain In Unit Test Project
        return referencesAssemblies
               .Prepend(rootAssembly)
               .SelectMany(x => x.DefinedTypes)
               .Where(x => x.ImplementedInterfaces.Contains(typeof(TInterface)))
               .ToList();//don't want to create an iterator with assemblies
    }

    #endregion

    #region Property Info Is Nullable<T>

    /// <summary>
    /// Is the property we pass in a nullable Of T..ie int?, int64?
    /// </summary>
    /// <param name="propertyToCheck">Property To Check</param>
    /// <returns>It is nullable of t</returns>
    /// <remarks>to get the underlying data type ie int32 or date use thisProperty.PropertyType.GetGenericArguments()[0]</remarks>
    public static bool IsNullableOfT(PropertyInfo propertyToCheck)
    {
        //to get the underlying type...int 32, or date, etc.
        //thisProperty.PropertyType.GetGenericArguments()[0]

        //go check the generic type
        return propertyToCheck.PropertyType.IsGenericType && propertyToCheck.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Checks the property and returns if it is an IEnumerable (A Collection)
    /// </summary>
    /// <param name="PropertyToCheck">Property Info To Check</param>
    /// <returns>Result If It's A Collection</returns>
    public static bool PropertyInfoIsIEnumerable(PropertyInfo propertyToCheck)
    {
        //we need to exclude strings because they implement IEnumerable...
        return (propertyToCheck.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyToCheck.PropertyType));
    }

    #endregion

}
