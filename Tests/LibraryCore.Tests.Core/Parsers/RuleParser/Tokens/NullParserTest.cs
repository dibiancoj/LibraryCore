﻿using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class NullParserTest : IClassFixture<RuleParserFixture>
{
    public NullParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine().ParseString("1 == null");

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken<int>>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NullToken>(result[4]);
    }

    [InlineData("$Name == null", null, true)]
    [InlineData("$Name == null", "abc", false)]
    [InlineData("$Name != null", null, false)]
    [InlineData("$Name != null", "abc", true)]
    [Theory]
    public void ExpressionsToTest(string expressionToTest, string nameValueToSet, bool expectedResult)
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine().ParseString(expressionToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder()
                                                       .WithName(nameValueToSet)
                                                       .Value));
    }

    [Fact]
    public void ExpressionInLinq()
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine().ParseString("$Name == null || $Name == 'Jacob'");
        var compiledExpression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey").Compile();

        var records = SurveyModelBuilder.CreateArrayOfRecords(
                                new SurveyModelBuilder()
                                    .WithName("Jacob"),

                                new SurveyModelBuilder()
                                    .WithName(null!),

                                new SurveyModelBuilder()
                                    .WithName("John"));

        var results = records.Where(compiledExpression);

        Assert.Equal(2, results.Count());
        Assert.Contains(results, x => x.Name == "Jacob");
        Assert.Contains(results, x => x.Name == null);
    }
}

