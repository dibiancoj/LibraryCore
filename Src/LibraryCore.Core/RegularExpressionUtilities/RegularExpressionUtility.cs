using LibraryCore.Core.ExtensionMethods;
using System.Text.RegularExpressions;

namespace LibraryCore.Core.RegularExpressionUtilities;

public static partial class RegularExpressionUtility
{

    #region Private Helper Methods

    private static TimeSpan DefaultTimeOut { get; } = new(0, 0, 3);

    #endregion

    #region Source Generators For Faster Perf

    private const string UrlIntoHyperLinkPattern = @"((http|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)";
    private const string ParseStringAndLeaveOnlyNumbersPattern = @"[\d-]";

#if NET6_0

    private static Regex ParseRawUrlIntoHyperLinkSourceGenerator() => new (UrlIntoHyperLinkPattern, RegexOptions.Compiled, DefaultTimeOut);
    private static Regex ParseStringAndLeaveOnlyNumbersSourceGenerator() => new(ParseStringAndLeaveOnlyNumbersPattern, RegexOptions.Compiled, DefaultTimeOut);

#else

    private const int DefaultTimeOutInMilliSeconds = 3000;

    //uses source generators which is alot faster
    [GeneratedRegex(UrlIntoHyperLinkPattern, RegexOptions.Compiled, DefaultTimeOutInMilliSeconds)]
    private static partial Regex ParseRawUrlIntoHyperLinkSourceGenerator(); //  <-- Declare the partial method, which will be implemented by the source generator

    [GeneratedRegex(ParseStringAndLeaveOnlyNumbersPattern, RegexOptions.Compiled, DefaultTimeOutInMilliSeconds)]
    private static partial Regex ParseStringAndLeaveOnlyNumbersSourceGenerator(); //  <-- Declare the partial method, which will be implemented by the source generator

#endif

    #endregion

    #region Parse A Raw Url To Blank Target

    /// <summary>
    /// Takes any link like strings ie: https://wwww.google.com and parses it into a a href link with a target blank
    /// </summary>
    /// <param name="htmlToParse">html to parse into a hyperlink</param>
    /// <returns>converted html</returns>
    public static string? ParseRawUrlIntoHyperLink(string? htmlToParse)
    {
        return htmlToParse.IsNullOrEmpty() ?
                htmlToParse :
                ParseRawUrlIntoHyperLinkSourceGenerator().Replace(htmlToParse, "<a target='_blank' href='$1'>$1</a>");
    }

    #endregion

    #region Wrap Search String

    /// <summary>
    /// Wrap a search string with a tag (html) or something else
    /// </summary>
    /// <param name="stringToInspect">Text to look in for the specific phrases</param>
    /// <param name="ignoreCase">Ignore case on the search</param>
    /// <param name="beginningReplacementTag">The start tag to replace the text with</param>
    /// <param name="endReplacementTag">The end tag to replace the text with</param>
    /// <param name="searchPhrases">phrases to look for and replace</param>
    /// <returns>The original string if not found. Otherwise the string with the appened beg and end tags</returns>
    public static string? WrapSearchPhraseInString(string? stringToInspect, bool ignoreCase, string beginningReplacementTag, string endReplacementTag, params string[] searchPhrases)
    {
        //exit if there is no text right away
        if (stringToInspect.IsNullOrEmpty())
        {
            return stringToInspect;
        }

        //build up the regex pattern. Should be "(SearchPhrase1)|(SearchPhrase2)"
        var searchPhrasePattern = string.Join('|', searchPhrases.Select(x => $"({x})"));

        var options = ignoreCase ?
                                 RegexOptions.IgnoreCase :
                                 RegexOptions.None;

        var regExToRun = new Regex(searchPhrasePattern, options | RegexOptions.Compiled, DefaultTimeOut);

        return regExToRun.Replace(stringToInspect, $"{beginningReplacementTag}$0{endReplacementTag}");
    }

    #endregion

    #region Only Number Parser

    /// <summary>
    /// Take a string and replace only the text (any non numeric text) in that string. ie 123jason945 will return 123ReplaceValue945
    /// </summary>
    /// <param name="stringToParse">String To Parse</param>
    /// <param name="replaceValue">Text You Want To Replace The Non Numeric Data Found</param>
    /// <returns>string with only numbers found in the original string</returns>
    public static string ParseStringAndLeaveOnlyNumbers(string stringToParse, string replaceValue)
    {
        return ParseStringAndLeaveOnlyNumbersSourceGenerator().Replace(stringToParse, replaceValue);
    }

    #endregion

}
