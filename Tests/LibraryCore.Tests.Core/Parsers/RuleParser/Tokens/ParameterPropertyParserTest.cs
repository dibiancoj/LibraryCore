using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class ParameterPropertyParserTest : IClassFixture<RuleParserFixture>
{
    public ParameterPropertyParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseParameterWithOneParameterTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine().ParseString("$Survey.PatientName == 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<ParameterPropertyToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<int>>(result[4]);

        var firstParameterToken = (ParameterPropertyToken)result[0];

        Assert.Equal("Survey", firstParameterToken.ParameterName);
        Assert.Equal("PatientName", firstParameterToken.PropertyName);
    }

    [Fact]
    public void ParseParameterWithShortHandParameterTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine().ParseString("$PatientName == 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<ParameterPropertyToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<int>>(result[4]);

        var firstParameterToken = (ParameterPropertyToken)result[0];

        Assert.Null(firstParameterToken.ParameterName);
        Assert.Equal("PatientName", firstParameterToken.PropertyName);
    }

    [InlineData("$Name == 'John Portal'", false)]
    [InlineData("$Name == 'Jacob DeGrom'", true)]
    [Theory]
    public void EqualExpression(string expressionToTest, bool expectedResult)
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine().ParseString(expressionToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder().Value));
    }

    [Fact]
    public void NonObjectParameter()
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine().ParseString("$Size == 25");
        var expression = RuleParserExpressionBuilder.BuildExpression<int>(tokens, "Size");

        Assert.True(expression.Compile().Invoke(25));
    }

    [Fact]
    public void PropertyNamePositiveRule()
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine().ParseString("$Survey.SurgeryCount == 10");
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(new SurveyModelBuilder().Value));
    }

    [Fact]
    public void PropertyNameWithOneParameterWhichIsNotSpecifiedPositiveRule()
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine().ParseString("$SurgeryCount == 10");
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.True(expression.Compile().Invoke(new SurveyModelBuilder().Value));
    }

    [Fact]
    public void EqualExpressionInLinq()
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine().ParseString("$Name == 'Jacob DeGrom'");
        var compiledExpression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey").Compile();

        var records = SurveyModelBuilder.CreateArrayOfRecords(
                                new SurveyModelBuilder(),
                                new SurveyModelBuilder()
                                    .WithName("John"));

        var results = records.Where(compiledExpression);

        Assert.Single(results);
    }
}

