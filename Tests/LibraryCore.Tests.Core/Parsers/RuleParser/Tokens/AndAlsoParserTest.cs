using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class AndAlsoParserTest : IClassFixture<RuleParserFixture>
{
    public AndAlsoParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParserTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine().ParseString("1 == 1 && 2 == 2");

        Assert.Equal(13, result.Count);
        Assert.IsType<NumberToken<int>>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<int>>(result[4]);

        Assert.IsType<WhiteSpaceToken>(result[5]);
        Assert.IsType<AndAlsoToken>(result[6]);
        Assert.IsType<WhiteSpaceToken>(result[7]);

        Assert.IsType<NumberToken<int>>(result[8]);
        Assert.IsType<WhiteSpaceToken>(result[9]);
        Assert.IsType<EqualsToken>(result[10]);
        Assert.IsType<WhiteSpaceToken>(result[11]);
        Assert.IsType<NumberToken<int>>(result[12]);
    }

    [Fact]
    public void LessThenOrEqualTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new AndAlsoToken().CreateExpression(Array.Empty<ParameterExpression>()));

    //string
    [InlineData("$Name == 'John Portal' && $Name == 'Bob'", false)]
    [InlineData("$Name == 'John Portal' && $Name == 'Jacob DeGrom'", false)]
    [InlineData("$Name == 'Jacob DeGrom' && $SurgeryCount == 100", false)]
    [InlineData("$Name == 'Jacob DeGrom' && $SurgeryCount == 10", true)]

    //numbers
    [InlineData("$SurgeryCount == 50 && $SurgeryCount == 10", false)]
    [InlineData("$SurgeryCount == 10 && $PriceOfSurgery == 9.99d", true)]

    [Theory]
    public void EqualExpression(string expressionToTest, bool expectedResult)
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine().ParseString(expressionToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder().Value));
    }

}

