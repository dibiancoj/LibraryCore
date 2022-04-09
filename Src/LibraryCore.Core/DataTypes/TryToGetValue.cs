using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.Core.DataTypes;

/// <summary>
/// Generic model for TryParse, TryGet, etc. where T will have a valud if result is true.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="ItemWasFound">Was the item found. If this is true then ItemValueIfFound should be accepted as the value found.</param>
/// <param name="ItemValueIfFound">Item Value if the value was found. If ItemWasFound then this value will be populated with the correct value from the data store or wherever this is pulling from</param>
public record TryToGetValue<T>(bool ItemWasFound, T? ItemValueIfFound)
{
    /// <summary>
    /// Tries to get the item from the source. Will return the result if it was found.
    /// </summary>
    /// <param name="itemFound">The item from source if found</param>
    /// <returns>If the item was found in session (yes / no)</returns>
    public bool GetValueIfFound([NotNullWhen(true)] out T? itemFound)
    {
        itemFound = ItemValueIfFound;
        return ItemWasFound;
    }
}
