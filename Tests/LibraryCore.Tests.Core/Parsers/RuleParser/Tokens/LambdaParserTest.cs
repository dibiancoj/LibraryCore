using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class LambdaParserTest : IClassFixture<RuleParserFixture>
{
    public LambdaParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseTestFromArrayConstant()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("[1,2,3].Any($t$ => $t$ == 200 || $t$ == 3) == true")
                                               .BuildExpression();

        Assert.True(expression.Compile().Invoke());
    }

    [Fact]
    public void ParseTestFromMethodResult()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("@GetAnswerArray($Survey$).Any($x$ => $x$ == 200 || $x$ == 3) == true")
                                               .BuildExpression<Survey>("Survey");

        Assert.True(expression.Compile().Invoke(null!));
    }

    [Fact]
    public void ParseTestFromParameterResult()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Lst$.Any($x$ => $x$ == 200 || $x$ == 3) == true")
                                               .BuildExpression<Survey, IEnumerable<int>>("Survey", "Lst");

        Assert.True(expression.Compile().Invoke(new SurveyModelBuilder().Value, new int[] { 1, 2, 3 }));
    }


    [Fact]
    public void ParseTestWithSubPropertyInsideLambda()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Surveys$.Any($x$ => $x.SurgeryCount$ == 24 || $x.Name$ == 'MySurvey2') == true")
                                               .BuildExpression<IEnumerable<Survey>>("Surveys");

        var model = new SurveyModelBuilder()
                            .WithName("MySurvey")
                            .WithSurgeryCount(24)
                            .Value;

        Assert.True(expression.Compile().Invoke(new[] { model }));
    }

    [Fact]
    public void ParseTestUsingCountLinqMethod()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Surveys$.Count($x$ => $x.SurgeryCount$ == 24 || $x.Name$ == 'MySurvey2') >= 1")
                                               .BuildExpression<IEnumerable<Survey>>("Surveys");

        var model = new SurveyModelBuilder()
                          .WithName("MySurvey")
                          .WithSurgeryCount(24)
                          .Value;

        Assert.True(expression.Compile().Invoke(new[] { model }));
    }

    [Fact]
    public void ParseTestWithChainedMethods()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Surveys$.Where($x$ => $x.SurgeryCount$ == 24 || $x.Name$ == 'MySurvey2').Count($x$ => true == true) >= 1")
                                               .BuildExpression<IEnumerable<Survey>>("Surveys");

        var model = new SurveyModelBuilder()
                       .WithName("MySurvey")
                       .WithSurgeryCount(24)
                       .Value;

        Assert.True(expression.Compile().Invoke(new[] { model }));
    }

    [Fact]
    public void MultipleClosingBracketsInLambda()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Surveys$.Count($x$ => $x.Name$.ToUpper() == 'MYSURVEY') >= 1")
                                               .BuildExpression<IEnumerable<Survey>>("Surveys");

        var model = new SurveyModelBuilder()
                          .WithName("MySurvey")
                          .WithSurgeryCount(24)
                          .Value;

        Assert.True(expression.Compile().Invoke(new[] { model }));
    }

    [Fact]
    public void WithStaticMethodCallInside()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Surveys$.Count($x$ => @GetANumberWithNoParameters() == 24) >= 1")
                                               .BuildExpression<IEnumerable<Survey>>("Surveys");

        var model = new SurveyModelBuilder()
                          .WithName("MySurvey")
                          .WithSurgeryCount(24)
                          .Value;

        Assert.True(expression.Compile().Invoke(new[] { model }));
    }

    [Fact]
    public void WithStaticMethodCallInsideAdvanced()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Surveys$.Any($x$ => @GetNameWithParameter($x$) == 'MySurvey') == true")
                                               .BuildExpression<IEnumerable<Survey>>("Surveys");

        var model = new SurveyModelBuilder()
                          .WithName("MySurvey")
                          .Value;

        Assert.True(expression.Compile().Invoke(new[] { model }));
    }
}

