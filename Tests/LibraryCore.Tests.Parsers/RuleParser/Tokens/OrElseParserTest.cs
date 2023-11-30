using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Parsers.RuleParser.Fixtures;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Parsers.RuleParser.Tokens;

public class OrElseParserTest : IClassFixture<RuleParserFixture>
{
    public OrElseParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParserTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("1 == 1 || 2 == 2")
                                            .CompilationTokenResult;

        Assert.Equal(13, result.Count);
        _ = Assert.IsType<NumberToken<int>>(result[0]);
        _ = Assert.IsType<WhiteSpaceToken>(result[1]);
        _ = Assert.IsType<EqualsToken>(result[2]);
        _ = Assert.IsType<WhiteSpaceToken>(result[3]);
        _ = Assert.IsType<NumberToken<int>>(result[4]);

        _ = Assert.IsType<WhiteSpaceToken>(result[5]);
        _ = Assert.IsType<OrElseToken>(result[6]);
        _ = Assert.IsType<WhiteSpaceToken>(result[7]);

        _ = Assert.IsType<NumberToken<int>>(result[8]);
        _ = Assert.IsType<WhiteSpaceToken>(result[9]);
        _ = Assert.IsType<EqualsToken>(result[10]);
        _ = Assert.IsType<WhiteSpaceToken>(result[11]);
        _ = Assert.IsType<NumberToken<int>>(result[12]);
    }

    [Fact]
    public void CreateTokenNotImplemented() => Assert.Throws<NotImplementedException>(() => new OrElseToken().CreateExpression(ImmutableList<ParameterExpression>.Empty));

    //string
    [InlineData("$Survey.Name$ == 'John Portal' || $Survey.Name$ == 'Bob'", false)]
    [InlineData("$Survey.Name$ == 'John Portal' || $Survey.Name$ == 'Jacob DeGrom'", true)]
    [InlineData("$Survey.SurgeryCount$ == 50 || $Survey.SurgeryCount$ == 100", false)]

    //int
    [InlineData("$Survey.SurgeryCount$ == 10 || $Survey.Name$ == 'Bob'", true)]
    [InlineData("$Survey.SurgeryCount$ == 100 || $Survey.Name$ == 'Jacob DeGrom'", true)]
    [InlineData("$Survey.SurgeryCount$ == 50 || $Survey.SurgeryCount$ == 10", true)]
    [InlineData("$Survey.SurgeryCount$ == 50 || $Survey.SurgeryCount$ == 100 || $Survey.SurgeryCount$ == 10", true)]

    [Theory]
    public void EqualExpression(string expressionToTest, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString(expressionToTest)
                                            .BuildExpression<Survey>("Survey")
                                            .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder().Value));
    }

}

