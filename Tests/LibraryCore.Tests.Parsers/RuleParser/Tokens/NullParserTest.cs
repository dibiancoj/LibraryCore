using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Parsers.RuleParser.Tokens;

public class NullParserTest(RuleParserFixture ruleParserFixture) : IClassFixture<RuleParserFixture>
{
    [Fact]
    public void ParseTest()
    {
        var result = ruleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("1 == null")
                                            .CompilationTokenResult;

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken<int>>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NullToken>(result[4]);
    }

    [InlineData("$Survey.NullableNameTest$ == null", null, true)]
    [InlineData("$Survey.NullableNameTest$ == null", "abc", false)]
    [InlineData("$Survey.NullableNameTest$ != null", null, false)]
    [InlineData("$Survey.NullableNameTest$ != null", "abc", true)]
    [Theory]
    public void ExpressionsToTest(string expressionToTest, string? nameValueToSet, bool expectedResult)
    {
        var expression = ruleParserFixture.ResolveRuleParserEngine()
                                                .ParseString(expressionToTest)
                                                .BuildExpression<Survey>("Survey")
                                                .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder()
                                                       .WithNullableNameTest(nameValueToSet)
                                                       .Value));
    }

    [Fact]
    public void ExpressionInLinq()
    {
        var expression = ruleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.Name$ == null || $Survey.Name$ == 'Jacob'")
                                            .BuildExpression<Survey>("Survey")
                                            .Compile();

        var records = SurveyModelBuilder.CreateArrayOfRecords(
                                new SurveyModelBuilder()
                                    .WithName("Jacob"),

                                new SurveyModelBuilder()
                                    .WithName(null!),

                                new SurveyModelBuilder()
                                    .WithName("John"));

        var results = records.Where(expression);

        Assert.Equal(2, results.Count());
        Assert.Contains(results, x => x.Name == "Jacob");
        Assert.Contains(results, x => x.Name == null);
    }
}

