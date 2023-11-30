using LibraryCore.Tests.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Parsers.RuleParser;

public class ExpressionBuilderTest(RuleParserFixture ruleParserFixture) : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; } = ruleParserFixture;

    [Fact]
    public void NoParameterTest()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 == 2")
                                                .BuildExpression();

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

    [InlineData(5, false)]
    [InlineData(30, true)]
    [Theory]
    public void SimpleParameterTest(int age, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("$Age$ > 15")
                                                .BuildExpression<int>("Age")
                                                .Compile();

        Assert.Equal(expectedResult, expression.Invoke(age));
    }

    [Fact]
    public void TwoParameterTest()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 25 && $Size.SurgeryCount$ == 12 || $Survey.SurgeryCount$ == 24")
                                            .BuildExpression<Survey, Survey>("Survey", "Size");

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
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 30 && $Size.SurgeryCount$ == 12 && $Color.SurgeryCount$ == 15")
                                            .BuildExpression<Survey, Survey, Survey>("Survey", "Size", "Color");

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
