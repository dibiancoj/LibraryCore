using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class OrElseParserTest : IClassFixture<RuleParserFixture>
{
    public OrElseParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParserTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 == 1 || 2 == 2");

        Assert.Equal(13, result.Count);
        Assert.IsType<NumberToken<int>>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<int>>(result[4]);

        Assert.IsType<WhiteSpaceToken>(result[5]);
        Assert.IsType<OrElseToken>(result[6]);
        Assert.IsType<WhiteSpaceToken>(result[7]);

        Assert.IsType<NumberToken<int>>(result[8]);
        Assert.IsType<WhiteSpaceToken>(result[9]);
        Assert.IsType<EqualsToken>(result[10]);
        Assert.IsType<WhiteSpaceToken>(result[11]);
        Assert.IsType<NumberToken<int>>(result[12]);
    }

    [Fact]
    public void LessThenOrEqualTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new OrElseToken().CreateExpression(Array.Empty<ParameterExpression>()));

    //string
    [InlineData("$Name == 'John Portal' || $Name == 'Bob'", false)]
    [InlineData("$Name == 'John Portal' || $Name == 'Jacob DeGrom'", true)]
    [InlineData("$SurgeryCount == 50 || $SurgeryCount == 100", false)]

    //int
    [InlineData("$SurgeryCount == 10 || $Name == 'Bob'", true)]
    [InlineData("$SurgeryCount == 100 || $Name == 'Jacob DeGrom'", true)]
    [InlineData("$SurgeryCount == 50 || $SurgeryCount == 10", true)]
    [InlineData("$SurgeryCount == 50 || $SurgeryCount == 100 || $SurgeryCount == 10", true)]

    [Theory]
    public void EqualExpression(string expressionToTest, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(expressionToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder().Value));
    }

}

