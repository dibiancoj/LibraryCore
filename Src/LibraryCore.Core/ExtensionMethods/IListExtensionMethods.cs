namespace LibraryCore.Core.ExtensionMethods;

public static class IListExtensionMethods
{

    /// <summary>
    /// Shift elements to the right in a low level array operation
    /// </summary>
    public static void ShiftRight<T>(this IList<T> collection, int indexToStartShift)
    {
        for (int i = collection.Count - 1; i > indexToStartShift; i--)
        {
            collection[i] = collection[i - 1];
        }
    }

    /// <summary>
    /// Shift elements to the left in a low level array operation
    /// </summary>
    public static void ShiftLeft<T>(this IList<T> collection, int indexToStartShift)
    {
        for (var i = indexToStartShift; i < collection.Count - 1; i++)
        {
            collection[i] = collection[i + 1];
        }
    }

}
