using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class StringParserTest : IClassFixture<RuleParserFixture>
{
    public StringParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("'abc' == 'def'")
                                                .CompilationTokenResult;

        Assert.Equal(5, result.Count);
        Assert.IsType<StringToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<StringToken>(result[4]);

        var firstStringToken = (StringToken)result[0];
        var lastStringToken = (StringToken)result[4];

        Assert.Equal("abc", firstStringToken.Value);
        Assert.Equal("def", lastStringToken.Value);
    }

    [Fact]
    public void ParseTestWithInnerFormatter()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("'abc {@GetANumberWithNoParameters()}'");

        Assert.Equal(1, result.CompilationTokenResult.Count);
        Assert.IsType<StringToken>(result.CompilationTokenResult[0]);

        var firstStringToken = (StringToken)result.CompilationTokenResult.Single();

        Assert.Equal("abc {0}", firstStringToken.Value);

        //compile it
        Assert.Equal("abc 24", result.BuildStringExpression().Compile().Invoke());
    }

    [Fact]
    public void StringWithNoClosingBracket()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.ResolveRuleParserEngine().ParseString("$Survey.Name$ == 'noclosingbracket"));

        Assert.Equal("Missing closing quote on String Value. Current Value = noclosingbracket", result.Message);
    }

    [InlineData("$Survey.Name$ == 'John Portal'", false)]
    [InlineData("$Survey.Name$ == 'Jacob DeGrom'", true)]
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
    public void ExpressionInLinq()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.Name$ == 'Jacob DeGrom'")
                                            .BuildExpression<Survey>("Survey")
                                            .Compile();

        var records = SurveyModelBuilder.CreateArrayOfRecords(
                                new SurveyModelBuilder(),
                                new SurveyModelBuilder()
                                    .WithName("John"));

        Assert.Single(records.Where(expression));
    }
}

