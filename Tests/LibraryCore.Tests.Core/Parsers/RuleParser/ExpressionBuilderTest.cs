using LibraryCore.Core.Parsers.RuleParser;
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
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("1 == 2");
        var expression = RuleParserExpressionBuilder.BuildExpression(tokens);

        Assert.False(expression.Compile().Invoke());
    }

    [Fact]
    public void TwoParameterTest()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.SurgeryCount == 24 && $Size.SurgeryCount == 12");
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey, Survey>(tokens, "Survey", "Size");

        Assert.True(expression.Compile().Invoke(new SurveyModelBuilder()
                                                .WithSurgeryCount(24)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value));
    }

    [Fact]
    public void ThreeParameterTest()
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.SurgeryCount == 30 && $Size.SurgeryCount == 12 && $Color.SurgeryCount == 15");
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
}
