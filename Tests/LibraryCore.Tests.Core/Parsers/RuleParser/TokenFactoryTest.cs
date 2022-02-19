using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class TokenFactoryTest : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; }

    public TokenFactoryTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    [Fact]
    public void NoFactoryFoundTest()
    {
        var exceptionThrown = Assert.Throws<Exception>(() =>
        {
            _ = RuleParserFixture.RuleParserEngineToUse.ParseString("true ** true");
        });

        Assert.Contains("No Token Found For Value = **", exceptionThrown.Message);
    }
}

