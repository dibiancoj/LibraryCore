using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Parsers.RuleParser.Tokens;

public class StringParserTest(RuleParserFixture ruleParserFixture) : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; } = ruleParserFixture;

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

        Assert.Single(result.CompilationTokenResult);
        Assert.IsType<StringToken>(result.CompilationTokenResult[0]);

        var firstStringToken = (StringToken)result.CompilationTokenResult.Single();

        Assert.Equal("abc {0}", firstStringToken.Value);

        //compile it
        Assert.Equal("abc 24", result.BuildStringExpression().Compile().Invoke());
    }

    [Fact]
    public void ParseTestWithInnerFormatterWithOneParameter()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("'abc {@GetANumberWithNoParameters()} | {$param1$}'");

        Assert.Single(result.CompilationTokenResult);
        Assert.IsType<StringToken>(result.CompilationTokenResult[0]);

        var firstStringToken = (StringToken)result.CompilationTokenResult.Single();

        Assert.Equal("abc {0} | {1}", firstStringToken.Value);

        //compile it
        Assert.Equal("abc 24 | 25", result.BuildStringExpression<int>("param1").Compile().Invoke(25));
    }

    [Fact]
    public void ParseTestWithInnerFormatterWithTwoParameter()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("'abc {@GetANumberWithNoParameters()} | {$param1$} | {$param2$}'");

        Assert.Single(result.CompilationTokenResult);
        Assert.IsType<StringToken>(result.CompilationTokenResult[0]);

        var firstStringToken = (StringToken)result.CompilationTokenResult.Single();

        Assert.Equal("abc {0} | {1} | {2}", firstStringToken.Value);

        //compile it
        Assert.Equal("abc 24 | 25 | True", result.BuildStringExpression<int, bool>("param1", "param2").Compile().Invoke(25, true));
    }

    [Fact]
    public void ParseTestWithInnerFormatterWithTwoParameterWithString()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("'abc {@GetANumberWithNoParameters()} | {$param1$} | {$param200$}'");

        Assert.Single(result.CompilationTokenResult);
        Assert.IsType<StringToken>(result.CompilationTokenResult[0]);

        var firstStringToken = (StringToken)result.CompilationTokenResult.Single();

        Assert.Equal("abc {0} | {1} | {2}", firstStringToken.Value);

        //compile it
        Assert.Equal("abc 24 | 25 | abcdefge", result.BuildStringExpression<int, string>("param1", "param200").Compile().Invoke(25, "abcdefge"));
    }

    [Fact]
    public void StringWithNoClosingBracket()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.ResolveRuleParserEngine().ParseString("$Survey.Name$ == 'noclosingbracket"));

        Assert.Equal("Missing closing quote on String Value. Current Value = noclosingbracket", result.Message);
    }

    [Fact]
    public void StringWithNoValueInsideInterpolation()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.ResolveRuleParserEngine().ParseString("$Survey.Name$ == 'Test{}'"));

        Assert.Equal("String Interpolation - No value found inside interpolation - {}", result.Message);
    }

    [Fact]
    public void EofAfterStartingBracketForInterpolation()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.ResolveRuleParserEngine().ParseString("$Survey.Name$ == 'Test{"));

        Assert.Equal("String Interpolation - No more characters to read after starting {", result.Message);
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

