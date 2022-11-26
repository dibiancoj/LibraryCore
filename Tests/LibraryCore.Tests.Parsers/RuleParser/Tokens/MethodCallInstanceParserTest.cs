using LibraryCore.Parsers.RuleParser.TokenFactories;
using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Parsers.RuleParser.Utilities;
using LibraryCore.Tests.Parsers.RuleParser.Fixtures;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Parsers.RuleParser.Tokens;

public class MethodCallInstanceParserTest : IClassFixture<RuleParserFixture>
{
    public MethodCallInstanceParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ThrowsOnCreateExpression()
    {
        Assert.Throws<NotImplementedException>(() => new MethodCallInstanceToken(new RuleParsingUtility.MethodParsingResult("UnitTest", ImmutableList<IToken>.Empty)).CreateExpression(ImmutableList<ParameterExpression>.Empty));
    }

    [Fact]
    public void ParseTestWithBaseDataType()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$str$.ToUpper() == 'TEST'")
                                               .BuildExpression<string>("str");

        Assert.True(expression.Compile().Invoke("TeSt"));
    }

    [Fact]
    public void ParseTestWithBaseDataTypeOnRightHandSide()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("'TEST' == $str$.ToUpper()")
                                               .BuildExpression<string>("str");

        Assert.True(expression.Compile().Invoke("TeSt"));
    }

    [Fact]
    public void ParseTestWithCustomDataType()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Survey$.InstanceMethodName(2) == 'My'")
                                               .BuildExpression<Survey>("Survey");

        Assert.True(expression.Compile().Invoke(new Survey("MySurvey", 24, default, default, default, default, default, default, default, default!, default)));
    }

    [Fact]
    public void ParseTestWithChainedMethods()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$str$.ToUpper().ToLower() == 'test'")
                                               .BuildExpression<string>("str");

        Assert.True(expression.Compile().Invoke("TeSt"));
    }
}

