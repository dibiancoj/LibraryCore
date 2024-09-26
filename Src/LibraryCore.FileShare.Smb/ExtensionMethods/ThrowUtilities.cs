using LibraryCore.FileShare.Smb.CustomExceptions;
using SMBLibrary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LibraryCore.Tests.FileShare.Smb")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace LibraryCore.FileShare.Smb.ExtensionMethods;

internal static class ThrowUtilities
{
    internal static NTStatus[] OnlySuccess { get; } = [NTStatus.STATUS_SUCCESS];

    internal static void ThrowIfNotSuccessful(this NTStatus value, [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        value.ThrowIfNotStatus(OnlySuccess, expression);
    }

    internal static void ThrowIfNotStatus(this NTStatus value, NTStatus[] statusValuesToAllow, [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        if (!statusValuesToAllow.Contains(value))
        {
            throw new SmbStatusException(value, expression);
        }
    }
}
