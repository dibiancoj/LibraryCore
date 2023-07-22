using LibraryCore.Core.ExtensionMethods;
using System.Text;

namespace LibraryCore.Tests.Core.ExtensionMethods;

public class StringExtensionMethodTest
{

    #region Contains

    #region Contains Off Of String

    /// <summary>
    /// Unit test string contains with string comparison (true result)
    /// </summary>
    [InlineData("Test", "TEST")]
    [InlineData("TEST", "TEST")]
    [InlineData("TEST2", "TEST")]
    [Theory]
    public void StringContainsTrueTest1(string ValueToTest, string ContainsValueToTest)
    {
        Assert.True(ValueToTest.Contains(ContainsValueToTest, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Unit test string contains with string comparison (false result)
    /// </summary>
    [InlineData("test 123", "BASEBALL")]
    [InlineData("123", "ABC123")]
    [Theory]
    public void StringContainsFalseTest1(string ValueToTest, string ContainsValueToTest)
    {
        Assert.False(ValueToTest.Contains(ContainsValueToTest, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Contains Off Of IEnumerable

    /// <summary>
    /// Unit test ienumerable of string contains with string comparison
    /// </summary>
    [Fact]
    public void IEnumerableStringContainsTest1()
    {
        //create a dummy string enumerator to test
        var ListToContainsWith = new string[] { "baseball1", "baseball2", "baseball3" };

        //make sure we can find some values
        Assert.True(ListToContainsWith.Contains("BASEBALL", StringComparison.OrdinalIgnoreCase));
        Assert.False(ListToContainsWith.Contains("BASEBALLABC", StringComparison.OrdinalIgnoreCase));

        //check ordinal now
        Assert.False(ListToContainsWith.Contains("BASEBALL", StringComparison.Ordinal));
        Assert.True(ListToContainsWith.Contains("baseball1", StringComparison.Ordinal));
    }

    #endregion

    #endregion

    #region String Is Null Or Empty - Instance

    /// <summary>
    /// Test if a string has a value in a string instance extension method (true result)
    /// </summary>
    [InlineData("Test")]
    [InlineData("123")]
    [InlineData("12345")]
    [Theory]
    public void HasValueTrueResultTest1(string ValueToTest)
    {
        Assert.True(ValueToTest.HasValue());
    }

    /// <summary>
    /// Test if a string has a value in a string instance extension method (false result)
    /// </summary>
    [InlineData("")]
    [InlineData((string?)null)]
    [Theory]
    public void HasValueFalseResultTest1(string? ValueToTest)
    {
        Assert.False(ValueToTest.HasValue());
    }

    /// <summary>
    /// Test is a string is null or empty in a string instance extension method (not null result)
    /// </summary>
    [InlineData(" ")]
    [InlineData("123")]
    [InlineData("123 456")]
    [InlineData("a")]
    [Theory]
    public void NullOrEmptyNotNullTestTest1(string ValueToTest)
    {
        Assert.False(ValueToTest.IsNullOrEmpty());
    }

    /// <summary>
    /// Test is a string is null or empty in a string instance extension method  (null result)
    /// </summary>
    [InlineData("")]
    [InlineData((string?)null)]
    [Theory]
    public void NullOrEmptyNullTestTest1(string? ValueToTest)
    {
        Assert.True(ValueToTest.IsNullOrEmpty());
    }

    #endregion

    #region String Is Null Or White Space

    [InlineData("", true)]
    [InlineData(null, true)]
    [InlineData("             ", true)]
    [InlineData(" ", true)]
    [InlineData("a ", false)]
    [InlineData("  a", false)]
    [InlineData("abc", false)]
    [Theory]
    public void IsNullOrWhiteSpaceTest1(string valueToTest, bool expectedResult)
    {
        Assert.Equal(expectedResult, valueToTest.IsNullOrWhiteSpace());
    }

    #endregion

    #region To Stream

    [Fact]
    public void ToMemoryStream()
    {
        var valueToStartWith = "Test abc";

        using var stream = valueToStartWith.ToMemoryStream();

        var backToString = Encoding.ASCII.GetString(stream.ToArray());
    }

    #endregion

    #region To Byte Array

    /// <summary>
    /// Test to make sure a string to a byte array is correct
    /// </summary>
    [Fact]
    public void StringToByteArrayTest1()
    {
        //loop through the elements to test using the helper method for this
        Assert.Equal(new byte[] { 106, 97, 115, 111, 110 }, "jason".ToByteArray());
    }

    /// <summary>
    /// Test to make sure a string to a byte array is correct
    /// </summary>
    [Fact]
    public void StringToByteArrayTest2()
    {
        //loop through the elements to test using the helper method for this
        Assert.Equal(new byte[] { 106, 97, 115, 111, 110, 50 }, "jason2".ToByteArray());
    }

    #endregion

    #region Surround With

    /// <summary>
    /// Test to make sure the SurroundWith works
    /// </summary>
    [InlineData("1234", "?1234?", "?")]
    [InlineData("Test", "!Test!", "!")]
    [Theory]
    public void SurroundWithTest1(string valueToTest, string shouldBeValue, string valueToSurroundWith)
    {
        Assert.Equal(shouldBeValue, valueToTest.SurroundWith(valueToSurroundWith));
    }

    /// <summary>
    /// Test to make sure the SurroundWithQuotes works
    /// </summary>
    [InlineData("12345", @"""12345""")]
    [InlineData("Test", @"""Test""")]
    [Theory]
    public void SurroundWithQuotesTest1(string valueToTest, string shouldBeValue)
    {
        Assert.Equal(shouldBeValue, valueToTest.SurroundWithQuotes());
    }

    #endregion

    #region To Base 64 Decode

    /// <summary>
    /// Test to make sure the Base 64 Encoding Works works
    /// </summary>
    [InlineData("Test12345hts")]
    [InlineData("456vsdfidsajf sdfds")]
    [Theory]
    public void Base64Encoded(string valueToTest)
    {
        Assert.Equal(valueToTest, valueToTest.ToBase64Encode().ToBase64Decode());
    }

    #endregion

    #region Format Zip Code

    /// <summary>
    /// Unit test for formatting a string to a usa zip code
    /// </summary>
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("10583", "10583")]
    [InlineData("1058322", "1058322")]
    [InlineData("105832233", "10583-2233")]
    [InlineData("12345678-", "12345678-")] //not valid should return whatever we passed in
    [Theory]
    public void FormatUSAZipCodeTest1(string valueToTest, string shouldBeValue)
    {
        Assert.Equal(shouldBeValue, valueToTest.ToUSAZipCode());
    }

    #endregion

    #region Format Phone Number

    /// <summary>
    /// Unit test for formatting a string to a usa phone number
    /// </summary>
    [InlineData(null, null)]
    [InlineData("9145552235", "(914) 555-2235")]
    [InlineData("(914) 555-2235", "(914) 555-2235")]
    [InlineData("(914)555-2235", "(914) 555-2235")]
    [InlineData("914552", "914552")]
    [Theory]
    public void FormatUSAPhoneNumberTest1(string valueToTest, string shouldBeValue)
    {
        Assert.Equal(shouldBeValue, valueToTest.ToUSAPhoneNumber());
    }

    #endregion

    #region All Indexes

    [Fact]
    public void AllIndexTestWith2Matches()
    {
        var result = "TTest".AllIndexes('T');

        Assert.Contains(0, result);
        Assert.Contains(1, result);
    }

    [Fact]
    public void AllIndexTestWithNoMatches()
    {
        Assert.Empty("TTest".AllIndexes('z'));
    }

    [Fact]
    public void AllIndexTestWith3Matches()
    {
        var result = "TTestT".AllIndexes('T');

        Assert.Contains(0, result);
        Assert.Contains(1, result);
        Assert.Contains(5, result);
    }

    #endregion

    #region Digits

    [InlineData("", 0)]
    [InlineData("abc", 0)]
    [InlineData("1abc", 1)]
    [InlineData("1abc2", 2)]
    [InlineData("(914)-552-2205", 10)]
    [InlineData("123456789", 9)]
    [Theory]
    public void HowManyDigitsInString(string stringToTest, int expectedInstancesOfADigit)
    {
        Assert.Equal(expectedInstancesOfADigit, stringToTest.NumberOfDigitsInTheString());
    }

    [InlineData("", "")]
    [InlineData("abc", "")]
    [InlineData("1abc", "1")]
    [InlineData("1abc2", "12")]
    [InlineData("(914)-552-2205", "9145522205")]
    [InlineData("123456789", "123456789")]
    [Theory]
    public void PullDigitsFromString(string stringToTest, string expectedResult)
    {
        Assert.Equal(expectedResult, new string(stringToTest.PullDigitsFromString().ToArray()));
    }

    #endregion

    #region Replace Tags

    [InlineData("no replacements", "no replacements")]
    [InlineData("123 [[item1]] 456", "123 item 1.1 456")] //1 replacement
    [InlineData("123 [[item1]] | [[item2]] 456", "123 item 1.1 | item 2.2 456")] //replace 2 tags
    [InlineData("123 [[item1]] [[item1]]", "123 item 1.1 item 1.1")] //replace the same tag twice
    [Theory]
    public void ReplaceAllTagsInString(string stringToSearchAndReplace, string expectedResult)
    {
        var replaceTags = new List<KeyValuePair<string, string>>
            {
                   new KeyValuePair<string, string>("[[item1]]", "item 1.1"),
                   new KeyValuePair<string, string>("[[item2]]", "item 2.2")
            };

        Assert.Equal(expectedResult, stringToSearchAndReplace.ReplaceAllTagsInString(replaceTags));
    }

    #endregion

    #region Throw If Null Or Empty

    [InlineData(true, null)]
    [InlineData(true, "")]
    [InlineData(false, " ")]
    [InlineData(false, "T")]
    [InlineData(false, "Test")]
    [Theory]
    public void ThrowIfNullOrEmpty(bool expectedToThrowException, string valueToTest)
    {
        if (expectedToThrowException)
        {
            var exception = Assert.Throws<ArgumentException>(() => valueToTest.ThrowIfNullOrEmpty());

            Assert.Equal("valueToTest must be string that is not null or empty", exception.Message);
        }
        else
        {
            valueToTest.ThrowIfNullOrEmpty();
        }
    }

    #endregion

}
