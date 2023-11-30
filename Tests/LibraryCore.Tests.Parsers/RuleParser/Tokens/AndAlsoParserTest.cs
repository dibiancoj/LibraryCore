using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Parsers.RuleParser.Fixtures;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Parsers.RuleParser.Tokens;

public class AndAlsoParserTest(RuleParserFixture ruleParserFixture) : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; } = ruleParserFixture;

    [Fact]
    public void ParserTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                .ParseString("1 == 1 && 2 == 2")
                                .CompilationTokenResult;

        Assert.Equal(13, result.Count);
        Assert.IsType<NumberToken<int>>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<int>>(result[4]);

        Assert.IsType<WhiteSpaceToken>(result[5]);
        Assert.IsType<AndAlsoToken>(result[6]);
        Assert.IsType<WhiteSpaceToken>(result[7]);

        Assert.IsType<NumberToken<int>>(result[8]);
        Assert.IsType<WhiteSpaceToken>(result[9]);
        Assert.IsType<EqualsToken>(result[10]);
        Assert.IsType<WhiteSpaceToken>(result[11]);
        Assert.IsType<NumberToken<int>>(result[12]);
    }

    [Fact]
    public void CreateTokenNotImplemented() => Assert.Throws<NotImplementedException>(() => new AndAlsoToken().CreateExpression(ImmutableList<ParameterExpression>.Empty));

    //string
    [InlineData("$Survey.Name$ == 'John Portal' && $Survey.Name$ == 'Bob'", false)]
    [InlineData("$Survey.Name$ == 'John Portal' && $Survey.Name$ == 'Jacob DeGrom'", false)]
    [InlineData("$Survey.Name$ == 'Jacob DeGrom' && $Survey.SurgeryCount$ == 100", false)]
    [InlineData("$Survey.Name$ == 'Jacob DeGrom' && $Survey.SurgeryCount$ == 10", true)]

    //numbers
    [InlineData("$Survey.SurgeryCount$ == 50 && $Survey.SurgeryCount$ == 10", false)]
    [InlineData("$Survey.SurgeryCount$ == 10 && $Survey.PriceOfSurgery$ == 9.99d", true)]

    [Theory]
    public void EqualExpression(string expressionToTest, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                            .ParseString(expressionToTest)
                            .BuildExpression<Survey>("Survey")
                            .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder().Value));
    }

    //3 and 4 statements to handle larger items
    [InlineData("$Survey.Name$ == 'Alex' && $Survey.Name$ == 'Bob' || $Survey.Name$ == 'Charlie'", false)]
    [InlineData("$Survey.Name$ == 'Alex' && $Survey.Name$ == 'Bob' || $Survey.Name$ == 'Charlie' || $Survey.Name$ == 'Jacob DeGrom'", true)]
    [InlineData("$Survey.Name$ == 'Alex' && $Survey.Name$ == 'Bob' || $Survey.Name$ == 'Charlie' || $Survey.Name$.ToUpper() == 'Jacob DeGrom'", false)]
    [InlineData("$Survey.Name$ == 'Alex' && $Survey.Name$ == 'Bob' || $Survey.Name$ == 'Charlie' || $Survey.Name$.ToUpper() == 'JACOB DEGROM'", true)]
    [InlineData("$Survey.Name$ == 'Alex' && $Survey.Name$ == 'Bob' && $Survey.Name$ == 'Charlie' && $Survey.Name$.ToUpper() == 'JACOB DEGROM'", false)]
    [Theory]
    public void ThreeEqualStatments(string expressionToTest, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                            .ParseString(expressionToTest)
                            .BuildExpression<Survey>("Survey")
                            .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder().Value));
    }

}

