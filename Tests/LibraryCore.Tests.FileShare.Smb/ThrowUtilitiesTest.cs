using LibraryCore.FileShare.Smb.CustomExceptions;
using LibraryCore.FileShare.Smb.ExtensionMethods;
using SMBLibrary;

namespace LibraryCore.Tests.FileShare.Smb;

public class ThrowUtilitiesTest
{
    [Fact]
    public void ThrowIfNotSuccessful_SuccessfulStatus() => NTStatus.STATUS_SUCCESS.ThrowIfNotSuccessful();

    [Fact]
    public void ThrowIfNotSuccessful_NotSuccessfulStatus() => Assert.Throws<SmbStatusException>(() => NTStatus.STATUS_END_OF_FILE.ThrowIfNotSuccessful());

    [InlineData(NTStatus.STATUS_END_OF_FILE)]
    [InlineData(NTStatus.STATUS_SUCCESS)]
    [Theory]
    public void ThrowIfNotStatus_SuccessfulStatus(NTStatus status) => status.ThrowIfNotStatus([NTStatus.STATUS_END_OF_FILE, .. ThrowUtilities.OnlySuccess]);

    [Fact]
    public void ThrowIfNotStatus_NotSuccessfulStatus()
    {
        foreach (var enumValue in Enum.GetValues<NTStatus>().Where(x => x != NTStatus.STATUS_SUCCESS && x != NTStatus.STATUS_END_OF_FILE))
        {
            Assert.Throws<SmbStatusException>(() => enumValue.ThrowIfNotStatus([NTStatus.STATUS_END_OF_FILE, .. ThrowUtilities.OnlySuccess]));
        }
    }
}
