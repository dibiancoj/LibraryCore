using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class NotEqualParserTest : IClassFixture<RuleParserFixture>
{
    public NotEqualParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
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
    public void NotEqualCreateTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new NotEqualsToken().CreateExpression(Array.Empty<ParameterExpression>()));

    [InlineData("$Name != null", null, false)]
    [InlineData("$Name != null", "abc", true)]
    [Theory]
    public void ExpressionsToTest(string expressionToTest, string nameValueToSet, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString(expressionToTest)
                                            .BuildExpression<Survey>("Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder()
                                                       .WithName(nameValueToSet)
                                                       .Value));
    }

    [Fact]
    public void ExpressionInLinq()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Name != null && $Name != 'Jacob'")
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

