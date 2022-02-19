using LibraryCore.Core.Parsers.RuleParser;
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
        var result = RuleParserFixture.ResolveRuleParserEngine().ParseString("'test' like 'tester'");

        Assert.Equal(5, result.Count);
        Assert.IsType<StringToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<LikeToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<StringToken>(result[4]);
    }

    [Fact]
    public void LikeTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new LikeToken().CreateExpression(Array.Empty<ParameterExpression>()));


    [InlineData("'abc' like 'def'", false)]
    [InlineData("'baseball' like 'base'", true)] //sql it's column name which is the longer text inside smaller text which we search for
    [Theory]
    public void StringLike(string code, bool expectedResult)
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine().ParseString(code);
        var expression = RuleParserExpressionBuilder.BuildExpression(tokens);

        Assert.Equal(expectedResult, expression.Compile().Invoke());
    }
}

