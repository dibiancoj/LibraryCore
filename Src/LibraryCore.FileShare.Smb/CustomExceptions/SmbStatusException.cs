using SMBLibrary;

namespace LibraryCore.FileShare.Smb.CustomExceptions;

public class SmbStatusException(NTStatus statusFound, string? Expression = null) : Exception
{
    public NTStatus StatusFound { get; } = statusFound;
    public string? Expression { get; } = Expression;

    public override string ToString()
    {
        return $"SMB Status Exception: StatusId = {StatusFound} - Expression = {Expression}";
    }
}
