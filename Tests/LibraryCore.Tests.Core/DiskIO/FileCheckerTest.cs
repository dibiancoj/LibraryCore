using LibraryCore.Core.DiskIO;
using System.Text;

namespace LibraryCore.Tests.Core.DiskIO;

public class FileCheckerTest
{
    [InlineData("MZ", true)]
    [InlineData("M", false)] //only 1 character
    [InlineData("123", false)]
    [InlineData("ab", false)]
    [InlineData("c", false)]
    [InlineData("ef", false)]
    [Theory]
    public void IsExecutablePositiveTest1(string FirstTwoBytesToTest, bool expectResultIsExe)
    {
        Assert.Equal(expectResultIsExe, FileChecker.IsExecutableInWindows(Encoding.ASCII.GetBytes(FirstTwoBytesToTest)));
    }
}
