using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class LikeParserTest : IClassFixture<RuleParserFixture>
{
    public LikeParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("'test' like 'tester'")
                                            .CompilationTokenResult;

        Assert.Equal(5, result.Count);
        Assert.IsType<StringToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<LikeToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<StringToken>(result[4]);
    }

    [Fact]
    public void CreateTokenNotImplemented() => Assert.Throws<NotImplementedException>(() => new LikeToken().CreateExpression(Array.Empty<ParameterExpression>()));


    [InlineData("$Name like 'John'", false)]
    [InlineData("$Name like 'John' || $SurgeryCount == 10", true)]
    [InlineData("$Name like 'Jacob'", true)]
    [Theory]
    public void LikeTest(string expressionToTest, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString(expressionToTest)
                                                .BuildExpression<Survey>("Survey")
                                                .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder().Value));
    }
}

