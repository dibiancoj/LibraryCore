using LibraryCore.Core.ThrowUtilities;

namespace LibraryCore.Tests.Core.ThrowUtilities;

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

    #region Array Guards

    [Fact]
    public void ThrowIfEnumerableIsNullOrEmpty_WithNull()
    {
        var exceptionThrown = Assert.Throws<ArgumentOutOfRangeException>(() => ThrowUtility.ThrowIfNullOrEmpty<int>(null!));

        Assert.Equal("The enumerable is null or empty (Parameter 'null')", exceptionThrown.Message);
    }

    [Fact]
    public void ThrowIfEnumerableIsNullOrEmpty_WithEmpty()
    {
        var exceptionThrown = Assert.Throws<ArgumentOutOfRangeException>(() => ThrowUtility.ThrowIfNullOrEmpty(Array.Empty<int>()));

        Assert.Equal("The enumerable is null or empty (Parameter 'Array.Empty<int>()')", exceptionThrown.Message);
    }

    [Fact]
    public void ErrorMessageIsCorrectWithVariable()
    {
        List<int> emptyArray = [];

        var exceptionThrown = Assert.Throws<ArgumentOutOfRangeException>(() => ThrowUtility.ThrowIfNullOrEmpty<int>(emptyArray));

        Assert.Equal("The enumerable is null or empty (Parameter 'emptyArray')", exceptionThrown.Message);
    }

    [Fact]
    public void DontThrowIfEnumerableHasElements()
    {
        ThrowUtility.ThrowIfNullOrEmpty([1, 2, 3]);

        Assert.True(true);
    }

    #endregion

}
