using LibraryCore.Core.MathUtilities;
using Xunit;

namespace LibraryCore.Tests.Core.MathUtilities;

public class MathUtilityTest
{
    [InlineData(false, 6)]
    [InlineData(true, 5)]
    [InlineData(false, 4)]
    [InlineData(true, 3)]
    [InlineData(true, 2)]
    [InlineData(true, 1)]
    [Theory]
    public void IsPrimeNumber(bool isPrimeNumberExpectedResult, int numberToCheck) => Assert.Equal(isPrimeNumberExpectedResult, MathUtility.IsPrimeNumber(numberToCheck));

    [InlineData(true, 6)]
    [InlineData(false, 5)]
    [InlineData(true, 4)]
    [InlineData(false, 3)]
    [InlineData(false, 2)]
    [InlineData(false, 1)]
    [Theory]
    public void IsCompositeNumber(bool isCompositeNumberExpectedResult, int numberToCheck) => Assert.Equal(isCompositeNumberExpectedResult, MathUtility.IsCompositeNumber(numberToCheck));
}
