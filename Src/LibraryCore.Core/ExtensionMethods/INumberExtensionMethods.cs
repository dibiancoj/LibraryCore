using System.Numerics;

namespace LibraryCore.Core.ExtensionMethods;

/// <summary>
/// Extension methods for the INumber interface
/// </summary>
public static class INumberExtensionMethods
{

#if NET7_0_OR_GREATER

    /// <summary>
    /// Determine if a value is between two other values
    /// </summary>
    /// <typeparam name="T">Type of the number which needs to implement INumber</typeparam>
    /// <param name="value">value to determine if its between the others. The value must be greater thean or equal to the lower band and less then to the higher band</param>
    /// <param name="lower">The lower band which the value needs to be equal or above</param>
    /// <param name="upper">The upper band which the value needs to be below</param>
    /// <returns>Boolean if the number is between the lower and upper value</returns>
    public static bool IsBetween<T>(this T value, T lower, T upper)
        where T : INumber<T>
    {
        return value >= lower && value < upper;
    }

#endif
}
