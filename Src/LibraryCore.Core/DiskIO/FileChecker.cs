using System.Text;

namespace LibraryCore.Core.DiskIO;

public static class FileChecker
{
    /// <summary>
    /// Determines if a file is an executable by looking at the first 2 bytes. Handles scenario's where the user modifies the file extension. 
    /// </summary>
    /// <param name="fileToInspect">byte array which contains the file you want to inspect</param>
    /// <returns>Is it an executable</returns>
    public static bool IsExecutableInWindows(ReadOnlySpan<byte> fileToInspect)
    {
        //now convert that to a string and compare it against MZ (which is an executable in windows)
        return fileToInspect.Length >= 2 && Encoding.UTF8.GetString(fileToInspect[..2]) == "MZ";
    }
}
