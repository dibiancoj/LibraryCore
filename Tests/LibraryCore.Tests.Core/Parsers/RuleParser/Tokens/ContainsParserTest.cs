using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class ContainsParserTest : IClassFixture<RuleParserFixture>
{
    public ContainsParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("@MyMethod1() contains 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<MethodCallToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<ContainsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<int>>(result[4]);
    }

    [Fact]
    public void ContainsTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new ContainsToken().CreateExpression(Array.Empty<ParameterExpression>()));

    [InlineData("[1,2,3] contains $Survey.SurgeryCount", false)]
    [InlineData("[1,2,3, 10] contains $Survey.SurgeryCount", true)]
    [Theory]
    public void ArrayContainsInt(string code, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(code);
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder().Value));
    }

    [InlineData("['John', 'Bob'] contains $Survey.Name", false)]
    [InlineData("['Jacob DeGrom', 'Johnny Bench'] contains $Survey.Name", true)]
    [Theory]
    public void ArrayContainsString(string code, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(code);
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder().Value));
    }

    [InlineData("@GetAnswerArray() contains 20", false)]
    [InlineData("@GetAnswerArray() contains 2", true)]
    [Theory]
    public void ContainsFromMethod(string code, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(code);
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder().Value));
    }

}

