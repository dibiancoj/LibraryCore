using LibraryCore.Parsers.RuleParser.TokenFactories;
using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Parsers.RuleParser.Utilities;
using LibraryCore.Tests.Parsers.RuleParser.Fixtures;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Parsers.RuleParser.Tokens;

public class MethodCallInstanceParserTest(RuleParserFixture ruleParserFixture) : IClassFixture<RuleParserFixture>
{
    [Fact]
    public void ThrowsOnCreateExpression()
    {
        Assert.Throws<NotImplementedException>(() => new MethodCallInstanceToken(new RuleParsingUtility.MethodParsingResult("UnitTest", ImmutableList<IToken>.Empty)).CreateExpression(ImmutableList<ParameterExpression>.Empty));
    }

    [Fact]
    public void ParseTestWithBaseDataType()
    {
        var expression = ruleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$str$.ToUpper() == 'TEST'")
                                               .BuildExpression<string>("str");

        Assert.True(expression.Compile().Invoke("TeSt"));
    }

    [Fact]
    public void ParseTestWithBaseDataTypeOnRightHandSide()
    {
        var expression = ruleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("'TEST' == $str$.ToUpper()")
                                               .BuildExpression<string>("str");

        Assert.True(expression.Compile().Invoke("TeSt"));
    }

    [Fact]
    public void ParseTestWithCustomDataType()
    {
        var expression = ruleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Survey$.InstanceMethodName(2) == 'My'")
                                               .BuildExpression<Survey>("Survey");

        Assert.True(expression.Compile().Invoke(new Survey("MySurvey", 24, default, default, default, default, default, default, default, default!, default, default)));
    }

    [Fact]
    public void ParseTestWithChainedMethods()
    {
        var expression = ruleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$str$.ToUpper().ToLower() == 'test'")
                                               .BuildExpression<string>("str");

        Assert.True(expression.Compile().Invoke("TeSt"));
    }
}

