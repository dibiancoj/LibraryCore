using LibraryCore.Tests.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Parsers.RuleParser;

public class TokenFactoryTest(RuleParserFixture ruleParserFixture) : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; } = ruleParserFixture;

    [Fact]
    public void NoFactoryFoundTest()
    {
        var exceptionThrown = Assert.Throws<Exception>(() =>
        {
            _ = RuleParserFixture.ResolveRuleParserEngine().ParseString("true ** true");
        });

        Assert.Contains("No Token Found For Value = **", exceptionThrown.Message);
    }
}

