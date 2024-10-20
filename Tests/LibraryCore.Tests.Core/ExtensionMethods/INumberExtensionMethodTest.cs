using LibraryCore.Core.ExtensionMethods;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public class INumberExtensionMethodTest
{
#if NET7_0_OR_GREATER

    [InlineData(3, 1, 5, true)] //basic true test
    [InlineData(0, 1, 5, false)] //base false test (number lower then lower band)
    [InlineData(6, 1, 5, false)] //base false test (number higher then higher band)
    [InlineData(1, 1, 5, true)] //same number on lower band should be true
    [InlineData(5, 1, 5, false)] //same number on upper band should be false
    [Theory]
    public void IsNumberBetween_Int(int value, int lower, int upper, bool expected)
    {
        Assert.Equal(expected, value.IsBetween(lower, upper));
    }

    [InlineData(3, 1, 5, true)] //basic true test
    [InlineData(0, 1, 5, false)] //base false test (number lower then lower band)
    [InlineData(6, 1, 5, false)] //base false test (number higher then higher band)
    [InlineData(1, 1, 5, true)] //same number on lower band should be true
    [InlineData(5, 1, 5, false)] //same number on upper band should be false
    [Theory]
    public void IsNumberBetween_Double(double value, double lower, double upper, bool expected)
    {
        Assert.Equal(expected, value.IsBetween(lower, upper));
    }

    [InlineData(3, 1, 5, true)] //basic true test
    [InlineData(0, 1, 5, false)] //base false test (number lower then lower band)
    [InlineData(6, 1, 5, false)] //base false test (number higher then higher band)
    [InlineData(1, 1, 5, true)] //same number on lower band should be true
    [InlineData(5, 1, 5, false)] //same number on upper band should be false
    [Theory]
    public void IsNumberBetween_Float(float value, float lower, float upper, bool expected)
    {
        Assert.Equal(expected, value.IsBetween(lower, upper));
    }

#endif
}
