using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class ScoreParserTest : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; }

    public ScoreParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    [Fact]
    public void ScoreTest1()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseScoreNew<int>(
                                            new(5, "$Survey.SurgeryCount$ <= 5"),
                                            new(10, "$Survey.SurgeryCount$ <= 10"),
                                            new(20, "$Survey.SurgeryCount$ <= 20"));

        var raw = result.BuildScoreExpression<Survey>(ScoreToken.ScoringMode.FirstTrueRuleWins, "Survey");

        var compiled = raw.Compile();

        var result2 = compiled(new Survey("test", 6, 5, DateTime.Now, DateTime.Now, false, false, 0, 0, null!, null));

        Assert.Equal(10, result2);
        //new KeyValuePair<int, string>(7, "$Survey.SurgeryCount$ > 5 && $Survey.SurgeryCount$ < 10"));
        // new KeyValuePair<int, string>(10, "$Survey.SurgeryCount$ > 10"));

        //var expression = result.BuildScoreExpression<Survey>("Survey");
        //var comp = expression.Compile();
        //var result2 = expression.Compile().Invoke(new Survey("test", 6, 5, DateTime.Now, DateTime.Now, false, false, 0, 0, null!, null));

        //var t = result2;
    }
}
