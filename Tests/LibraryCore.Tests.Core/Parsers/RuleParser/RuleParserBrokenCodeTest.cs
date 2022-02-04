using LibraryCore.Core.Parsers.RuleParser;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class RuleParserBrokenCodeTest : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; }

    public RuleParserBrokenCodeTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    [Fact]
    public void StringWithNoClosingBracket()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.RuleParserEngineToUse.ParseString("$Name == 'noclosingbracket"));

        Assert.Equal("Missing closing quote on String Value. Current Value = noclosingbracket", result.Message);    
    }
}
