using System.Diagnostics.CodeAnalysis;

namespace LibraryCore.AspNet.SessionState;

public record TryToGetResult<T>(bool FoundInSession, T? ItemInSessionIfFound)
{
    /// <summary>
    /// Tries to get the item if in session. Will return the result if it was found in session
    /// </summary>
    /// <param name="itemFoundInSession">The item from sesion if found</param>
    /// <returns>If the item was found in session (yes / no)</returns>
    public bool GetItemIfFoundInSession([NotNullWhen(true)] out T? itemFoundInSession)
    {
        itemFoundInSession = ItemInSessionIfFound;
        return FoundInSession;
    }
}