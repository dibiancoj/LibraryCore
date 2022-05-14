using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class GreaterThenParserTest : IClassFixture<RuleParserFixture>
{
    public GreaterThenParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 > 1")
                                                .CompilationTokenResult;

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken<int>>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<GreaterThenToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<int>>(result[4]);
    }

    [Fact]
    public void CreateTokenNotImplemented() => Assert.Throws<NotImplementedException>(() => new GreaterThenToken().CreateExpression(ImmutableList<ParameterExpression>.Empty));

    [InlineData("$Survey.PriceOfSurgery$ > 9.99d", false)]
    [InlineData("$Survey.PriceOfSurgery$ > 8.25d", true)]
    [InlineData("$Survey.PriceOfSurgery$ > 60d", false)]
    [Theory]
    public void ExpressionsToTest(string expressionToTest, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString(expressionToTest)
                                                .BuildExpression<Survey>("Survey")
                                                .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder().Value));
    }
}

