using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.ThrowUtilities;

namespace LibraryCore.Tests.Core.ThrowUtilities
{
    public class ThrowUtilityTest
    {

        [InlineData(false, false)]
        [InlineData(true, true)]
        [Theory]
        public void ThrowIfTrue(bool expectedToThrowException, bool valueToTest)
        {
            if (expectedToThrowException)
            {
                Assert.Throws<ArgumentException>(() => ThrowUtility.ThrowIfTrue(valueToTest));
            }
        }

        [InlineData(true, false)]
        [InlineData(false, true)]
        [Theory]
        public void ThrowIfFalse(bool expectedToThrowException, bool valueToTest)
        {
            if (expectedToThrowException)
            {
                Assert.Throws<ArgumentException>(() => ThrowUtility.ThrowIfFalse(valueToTest));
            }
        }

    }
}
