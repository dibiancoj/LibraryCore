﻿using LibraryCore.Core.Parsers.RuleParser.ExpressionBuilders;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class ExpressionBuilderTest : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; }

    public ExpressionBuilderTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    [Fact]
    public void NoParameterTest()
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 == 2")
                                                .CompilationTokenResult;

        var expression = RuleParserExpressionBuilder.BuildExpression(tokens);

        Assert.False(expression.Compile().Invoke());
    }

    [Fact]
    public void NoParameterTestFromCompilationResult()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 == 2")
                                                .BuildExpression()
                                                .Compile();

        Assert.False(expression.Invoke());
    }

    [Fact]
    public void TwoParameterTest()
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 24 && $Size.SurgeryCount$ == 12")
                                            .CompilationTokenResult;

        var expression = RuleParserExpressionBuilder.BuildExpression<Survey, Survey>(tokens, "Survey", "Size");

        Assert.True(expression.Compile().Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(24)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value));
    }

    [Fact]
    public void TwoParameterTestWithCompilationResult()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 24 && $Size.SurgeryCount$ == 12")
                                            .BuildExpression<Survey, Survey>("Survey", "Size")
                                            .Compile();

        Assert.True(expression.Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(24)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value));
    }

    [Fact]
    public void ThreeParameterTest()
    {
        var tokens = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 30 && $Size.SurgeryCount$ == 12 && $Color.SurgeryCount$ == 15")
                                            .CompilationTokenResult;

        var expression = RuleParserExpressionBuilder.BuildExpression<Survey, Survey, Survey>(tokens, "Survey", "Size", "Color");

        Assert.True(expression.Compile().Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(30)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(15)
                                                .Value));
    }

    [Fact]
    public void ThreeParameterTestWithCompilationResult()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 30 && $Size.SurgeryCount$ == 12 && $Color.SurgeryCount$ == 15")
                                            .BuildExpression<Survey, Survey, Survey>("Survey", "Size", "Color")
                                            .Compile();

        Assert.True(expression.Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(30)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(15)
                                                .Value));
    }

}
