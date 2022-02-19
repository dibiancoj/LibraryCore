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
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("$Survey.PatientName == 1")
                                                .CompilationTokenResult;

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
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$PatientName == 1")
                                            .CompilationTokenResult;

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
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString(expressionToTest)
                                            .BuildExpression<Survey>("Survey")
                                            .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder().Value));
    }

    [Fact]
    public void NonObjectParameter()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Size == 25")
                                            .BuildExpression<int>("Size")
                                            .Compile();
            
        Assert.True(expression.Invoke(25));
    }

    [Fact]
    public void PropertyNamePositiveRule()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount == 10")
                                            .BuildExpression<Survey>("Survey")
                                            .Compile();

        Assert.True(expression.Invoke(new SurveyModelBuilder().Value));
    }

    [Fact]
    public void PropertyNameWithOneParameterWhichIsNotSpecifiedPositiveRule()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$SurgeryCount == 10")
                                            .BuildExpression<Survey>("Survey")
                                            .Compile();

        Assert.True(expression.Invoke(new SurveyModelBuilder().Value));
    }

    [Fact]
    public void EqualExpressionInLinq()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Name == 'Jacob DeGrom'")
                                            .BuildExpression<Survey>("Survey")
                                            .Compile();

        var records = SurveyModelBuilder.CreateArrayOfRecords(
                                new SurveyModelBuilder(),
                                new SurveyModelBuilder()
                                    .WithName("John"));

        Assert.Single(records.Where(expression));
    }
}

