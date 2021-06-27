using Xunit;

namespace LibraryCore.Tests.Core
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal("Hello World", LibraryCore.Core.Class1.Seed());
        }
    }
}
