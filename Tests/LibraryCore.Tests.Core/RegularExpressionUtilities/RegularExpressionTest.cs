using LibraryCore.Core.RegularExpressionUtilities;

namespace LibraryCore.Tests.Core.RegularExpressionUtilities;

public class RegularExpressionTest
{

    #region Parse Raw Url Into Hyperlink

    //regular strings
    [InlineData(null, null)]
    [InlineData("Test", "Test")]
    [InlineData("Test1 Test2", "Test1 Test2")]

    //http
    [InlineData("Test http://www.google.com", "Test <a target='_blank' href='http://www.google.com'>http://www.google.com</a>")]
    [InlineData("http://www.google.com Test", "<a target='_blank' href='http://www.google.com'>http://www.google.com</a> Test")]

    //https
    [InlineData("Test https://www.google123.com", "Test <a target='_blank' href='https://www.google123.com'>https://www.google123.com</a>")]
    [InlineData("https://www.google456.com Test", "<a target='_blank' href='https://www.google456.com'>https://www.google456.com</a> Test")]

    //local host - not an external url...should return the same thing
    [InlineData("local host test http://localhost/Test123", "local host test http://localhost/Test123")]
    [Theory]
    public void ParseRawUrlIntoHyperLinkTests(string? input, string? expectedOutput)
    {
        Assert.Equal(expectedOutput, RegularExpressionUtility.ParseRawUrlIntoHyperLink(input));
    }

    #endregion

    #region Wrap Search Phrase In String

    //null checks
    [InlineData(null, null, false, "", "", "Test 123")]
    [InlineData("", "", false, "", "", "Test 123")]

    //1 word no match
    [InlineData("dad 123", "dad 123", true, "<b>", "</b>", "abc")]

    //1 word - 1 match (same case)
    [InlineData("dad <b>abc</b>", "dad abc", false, "<b>", "</b>", "abc")]

    //shouldn't be a match because we want same case
    [InlineData("dad <b>abc</b>", "dad abc", true, "<b>", "</b>", "ABC")]

    //multiple matches (same case)
    [InlineData("dad <b>abc</b> aa<b>def</b>aa", "dad abc aadefaa", false, "<b>", "</b>", "abc", "def")]

    //multiple matches (ignore case)
    [InlineData("dad <b>abc</b> aa<b>def</b>aa", "dad abc aadefaa", true, "<b>", "</b>", "ABC", "dEf")]
    [Theory]
    public void WrapSearchPhraseInStringTest(string? expectedResult, string? stringToInspect, bool caseInsensitive, string beginningReplacementTag, string endReplacementTag, params string[] searchPhrases)
    {
        Assert.Equal(expectedResult, RegularExpressionUtility.WrapSearchPhraseInString(stringToInspect, caseInsensitive, beginningReplacementTag, endReplacementTag, searchPhrases));
    }

    #endregion

    #region NumberParser

    [InlineData("jason123", "jason", "")]
    [InlineData("123jason123", "jason", "")]
    [InlineData("123jas123on123", "jason", "")]
    [InlineData("123jas1on123", "zzzjaszonzzz", "z")]
    [Theory]
    public void NumberParserTest2(string testValueToParse, string shouldBeValue, string replaceWithValue)
    {
        Assert.Equal(shouldBeValue, RegularExpressionUtility.ParseStringAndLeaveOnlyNumbers(testValueToParse, replaceWithValue));
    }

    #endregion

}
