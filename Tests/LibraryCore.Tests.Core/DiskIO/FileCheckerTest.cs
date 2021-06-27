using LibraryCore.Core.DiskIO;
using System.Text;
using Xunit;

namespace LibraryCore.Tests.Core.DiskIO
{
    public class FileCheckerTest
    {
        [InlineData("MZ", true)]
        [InlineData("M", false)] //only 1 character
        [InlineData("123", false)]
        [InlineData("ab", false)]
        [InlineData("c", false)]
        [InlineData("ef", false)]
        [InlineData("sdfasdfasd", true)]
        [Theory]
        public void IsExecutablePositiveTest1(string FirstTwoBytesToTest, bool expectResultIsExe)
        {
            Assert.Equal(expectResultIsExe, FileChecker.IsExecutableInWindows(Encoding.ASCII.GetBytes(FirstTwoBytesToTest)));
        }
    }
}
