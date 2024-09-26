using LibraryCore.FileShare.Smb.Settings;
using System.Net;

namespace LibraryCore.Tests.FileShare.Smb;

public class MappedDriveSettingsTest : SmbFileShareSettings
{
    public static IPAddress IpAddressToUse { get; } = new([192, 168, 1, 1]);

    public override IPAddress ResolveIpAddress() => IpAddressToUse;
}