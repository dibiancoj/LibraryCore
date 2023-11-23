using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Parsers.RuleParser.Fixtures;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Parsers.RuleParser.Tokens;

public class NotEqualParserTest(RuleParserFixture ruleParserFixture) : IClassFixture<RuleParserFixture>
{
    [Fact]
    public void ParseTest()
    {
        var result = ruleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("1 != 1")
                                            .CompilationTokenResult;

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken<int>>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<NotEqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<int>>(result[4]);
    }

    [Fact]
    public void CreateTokenNotImplemented() => Assert.Throws<NotImplementedException>(() => new NotEqualsToken().CreateExpression(ImmutableList<ParameterExpression>.Empty));

    [InlineData("$Survey.NullableNameTest$ != null", null, false)]
    [InlineData("$Survey.NullableNameTest$ != null", "abc", true)]
    [Theory]
    public void ExpressionsToTest(string expressionToTest, string? nameValueToSet, bool expectedResult)
    {
        var expression = ruleParserFixture.ResolveRuleParserEngine()
                                            .ParseString(expressionToTest)
                                            .BuildExpression<Survey>("Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder()
                                                       .WithNullableNameTest(nameValueToSet)
                                                       .Value));
    }

    [Fact]
    public void ExpressionInLinq()
    {
        var expression = ruleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.Name$ != null && $Survey.Name$ != 'Jacob'")
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

        Assert.Single(results);
        Assert.Contains(results, x => x.Name == "John");
    }
}

