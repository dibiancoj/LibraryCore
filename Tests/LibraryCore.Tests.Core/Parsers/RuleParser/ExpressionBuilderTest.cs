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
    public void BlaRegularInstanceMethod()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$str$.ToUpper() == 'TEST'")
                                               .BuildExpression<string>("str");

        Assert.True(expression.Compile().Invoke("test"));
    }

    [Fact]
    public void Bla()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("[1,2,3].Any($t$ => $t$ == 200 || $t$ == 3) == true")
                                               .BuildExpression();

        Assert.True(expression.Compile().Invoke());
    }

    [Fact]
    public void Bla2()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("@GetAnswerArray($Survey$).Any($x$ => $x$ == 200 || $x$ == 3) == true")
                                               .BuildExpression<Survey>("Survey");

        Assert.True(expression.Compile().Invoke(null!));
    }

    [Fact]
    public void Bla3()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Lst$.Any($x$ => $x$ == 200 || $x$ == 3) == true")
                                               .BuildExpression<Survey, IEnumerable<int>>("Survey", "Lst");

        Assert.True(expression.Compile().Invoke(null!, new int[] { 1, 2, 3 }));
    }


    [Fact]
    public void Bla4()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Surveys$.Any($x$ => $x.SurgeryCount$ == 24 || $x.Name$ == 'MySurvey2') == true")
                                               .BuildExpression<IEnumerable<Survey>>("Surveys");

        Assert.True(expression.Compile().Invoke(new List<Survey> { new Survey("MySurvey", 24, default, default, default, default, default, default, default, default!, default) }));
    }

    [Fact]
    public void BlaCount1()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Surveys$.Count($x$ => $x.SurgeryCount$ == 24 || $x.Name$ == 'MySurvey2') >= 1")
                                               .BuildExpression<IEnumerable<Survey>>("Surveys");

        Assert.True(expression.Compile().Invoke(new List<Survey> { new Survey("MySurvey", 24, default, default, default, default, default, default, default, default!, default) }));
    }

    [Fact]
    public void BlaChain()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Surveys$.Where($x$ => $x.SurgeryCount$ == 24 || $x.Name$ == 'MySurvey2').Count($x$ => true == true) >= 1")
                                               .BuildExpression<IEnumerable<Survey>>("Surveys");

        Assert.True(expression.Compile().Invoke(new List<Survey> { new Survey("MySurvey", 24, default, default, default, default, default, default, default, default!, default) }));
    }

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
                                                .ParseString($"$Age$ > 15")
                                                .BuildExpression<int>("Age")
                                                .Compile();

        Assert.Equal(expectedResult, expression.Invoke(age));
    }

    [Fact]
    public void TwoParameterTest()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 24 && $Size.SurgeryCount$ == 12")
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
