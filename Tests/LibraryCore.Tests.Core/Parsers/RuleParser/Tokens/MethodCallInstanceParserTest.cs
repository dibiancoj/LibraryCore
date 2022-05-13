using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class MethodCallInstanceParserTest : IClassFixture<RuleParserFixture>
{
    public MethodCallInstanceParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseTest()
    {
        Assert.False(true);
        //var result = RuleParserFixture.ResolveRuleParserEngine()
        //                                     .ParseString("1 == 1")
        //                                     .CompilationTokenResult;

        //Assert.Equal(5, result.Count);
        //Assert.IsType<NumberToken<int>>(result[0]);
        //Assert.IsType<WhiteSpaceToken>(result[1]);
        //Assert.IsType<EqualsToken>(result[2]);
        //Assert.IsType<WhiteSpaceToken>(result[3]);
        //Assert.IsType<NumberToken<int>>(result[4]);
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
    public void BlaRegularInstanceMethod2()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Survey$.InstanceMethodName(2) == 'My'")
                                               .BuildExpression<Survey>("Survey");

        Assert.True(expression.Compile().Invoke(new Survey("MySurvey", 24, default, default, default, default, default, default, default, default!, default)));
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

    //[Fact]
    //public void CreateTokenNotImplemented() => Assert.Throws<NotImplementedException>(() => new EqualsToken().CreateExpression(Array.Empty<ParameterExpression>()));

    //[InlineData("$Survey.PriceOfSurgery$ == 9.99d", true)]
    //[InlineData("$Survey.PriceOfSurgery$ == 8.25d", false)]
    //[InlineData("$Survey.PriceOfSurgery$ == 60d", false)]
    //[InlineData("$Survey.Name$ == 'Jacob DeGrom'", true)]
    //[InlineData("$Survey.Name$ == 'Tommy'", false)]
    //[Theory]
    //public void ExpressionsToTest(string expressionToTest, bool expectedResult)
    //{
    //    var expression = RuleParserFixture.ResolveRuleParserEngine()
    //                                        .ParseString(expressionToTest)
    //                                        .BuildExpression<Survey>("Survey")
    //                                        .Compile();

    //    Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder().Value));
    //}
}

