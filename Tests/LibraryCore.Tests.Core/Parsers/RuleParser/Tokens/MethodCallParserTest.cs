﻿using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class MethodCallParserTest : IClassFixture<RuleParserFixture>
{
    public MethodCallParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("@MyMethod1(1,true, 'bla') == 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<MethodCallToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<int>>(result[4]);
    }

    [Fact]
    public void MethodNameNotRegistered()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.RuleParserEngineToUse.ParseString("$Id == @MethodNotRegistered()"));

        Assert.Equal("Method Name = MethodNotRegistered Is Not Registered In MethodCallFactory. Call RegisterNewMethodAlias To Register The Method", result.Message);
    }

    [Fact]
    public void MethodCallInvalidSyntax()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.RuleParserEngineToUse.ParseString("$Id == @"));

        Assert.Equal("MethodCallFactory Not Able To Parse Information", result.Message);
    }

    [InlineData(true, "@MyMethod1(1) == 'Yes'")]
    [InlineData(false, "@MyMethod1(1) == 'No'")]
    [InlineData(false, "@MyMethod1(1) != 'Yes'")]
    [InlineData(true, "@MyMethod1(2) == 'No'")]
    [InlineData(true, "@MyMethod1(1) == 'Yes' && @MyMethod1(2) == 'No'")]
    [InlineData(false, "@MyMethod1(1) == 'No' && @MyMethod1(2) == 'No'")]
    [InlineData(true, "@MyMethod1(1) == 'No' || @MyMethod1(2) == 'No'")]
    [InlineData(false, "@MyMethod1(1) == 'abc' || @MyMethod1(2) == 'def'")]
    [Theory]
    public void BasicMethodCallPositive(bool expectedResult, string clauseToTest)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(clauseToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder().Value));
    }

}

