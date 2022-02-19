using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class GreaterThenOrEqualParserTest : IClassFixture<RuleParserFixture>
{
    public GreaterThenOrEqualParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 >= 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken<int>>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<GreaterThenOrEqualToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<int>>(result[4]);
    }

    [Fact]
    public void GreaterThenEqualTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new GreaterThenOrEqualToken().CreateExpression(Array.Empty<ParameterExpression>()));

    [InlineData("$PriceOfSurgery >= 9.99d", true)]
    [InlineData("$PriceOfSurgery >= 8.25d", true)]
    [InlineData("$PriceOfSurgery >= 60d", false)]
    [Theory]
    public void ExpressionsToTest(string expressionToTest, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(expressionToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder().Value));
    }
}

