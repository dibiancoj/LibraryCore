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
                var exception = Assert.Throws<ArgumentException>(() => ThrowUtility.ThrowIfTrue(valueToTest));

                Assert.Equal("valueToTest must be False, but was True", exception.Message);
            }
            else
            {
                ThrowUtility.ThrowIfTrue(valueToTest);
            }
        }

        [InlineData(true, false)]
        [InlineData(false, true)]
        [Theory]
        public void ThrowIfFalse(bool expectedToThrowException, bool valueToTest)
        {
            if (expectedToThrowException)
            {
                var exception = Assert.Throws<ArgumentException>(() => ThrowUtility.ThrowIfFalse(valueToTest));

                Assert.Equal("valueToTest must be True, but was False", exception.Message);
            }
            else
            {
                ThrowUtility.ThrowIfFalse(valueToTest);
            }
        }

    }
}
