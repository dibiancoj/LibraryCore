using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class NumberParserTest : IClassFixture<RuleParserFixture>
{
    public NumberParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void IntParseTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("1 == 24")
                                            .CompilationTokenResult;

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken<int>>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<int>>(result[4]);

        var firstStringToken = (NumberToken<int>)result[0];
        var lastStringToken = (NumberToken<int>)result[4];

        Assert.Equal(1, firstStringToken.Value);
        Assert.Equal(24, lastStringToken.Value);
    }

    [Fact]
    public void DoubleParseTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("1d == 24d")
                                            .CompilationTokenResult;

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken<double>>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken<double>>(result[4]);

        var firstStringToken = (NumberToken<double>)result[0];
        var lastStringToken = (NumberToken<double>)result[4];

        Assert.Equal(1, firstStringToken.Value);
        Assert.Equal(24, lastStringToken.Value);
    }

    [Fact]
    public void IntNotParseable()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.ResolveRuleParserEngine().ParseString("$Id$ == 12345678912341231231232156789"));

        Assert.Equal("Number Factory [Int] Not Able To Parse Number. Value = 12345678912341231231232156789", result.Message);
    }

    [Fact]
    public void DoubleNotParseable()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.ResolveRuleParserEngine().ParseString("$Survey.Id$ == 12.32.23d"));

        Assert.Equal("Number Factory [Double] Not Able To Parse Number. Value = 12.32.23", result.Message);
    }

    [InlineData("$Survey.SurgeryCount$ >= 17", false)]
    [InlineData("$Survey.SurgeryCount$ >= 10", true)]
    [InlineData("$Survey.SurgeryCount$ > 10", false)]
    [InlineData("$Survey.SurgeryCount$ <= 10", true)]
    [InlineData("$Survey.SurgeryCount$ < 10", false)]
    [InlineData("$Survey.SurgeryCount$ == 5", false)]
    [InlineData("$Survey.SurgeryCount$ == 10", true)]
    [Theory]
    public void IntExpressionsToTest(string expressionToTest, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString(expressionToTest)
                                                .BuildExpression<Survey>("Survey")
                                                .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder().Value));
    }

    [InlineData("$Survey.PriceOfSurgery$ >= 17d", false)]
    [InlineData("$Survey.PriceOfSurgery$ >= 10d", false)]
    [InlineData("$Survey.PriceOfSurgery$ >= 9.99d", true)]
    [InlineData("$Survey.PriceOfSurgery$ > 10d", false)]
    [InlineData("$Survey.PriceOfSurgery$ <= 10d", true)]
    [InlineData("$Survey.PriceOfSurgery$ < 10d", true)]
    [InlineData("$Survey.PriceOfSurgery$ == 5d", false)]
    [InlineData("$Survey.PriceOfSurgery$ == 10d", false)]
    [InlineData("$Survey.PriceOfSurgery$ == 9.99d", true)]
    [Theory]
    public void DoubleExpressionsToTest(string expressionToTest, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString(expressionToTest)
                                            .BuildExpression<Survey>("Survey")
                                            .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder().Value));
    }

    [Fact]
    public void ExpressionInLinq()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                               .ParseString("$Survey.SurgeryCount$ == 10 || $Survey.PriceOfSurgery$ == 9.95d")
                                               .BuildExpression<Survey>("Survey")
                                               .Compile();

        var records = SurveyModelBuilder.CreateArrayOfRecords(

                                new SurveyModelBuilder()
                                    .WithName("1")
                                    .WithSurgeryCount(9)
                                    .WithSurgeryPrice(9.95),

                                new SurveyModelBuilder()
                                    .WithName("2")
                                    .WithSurgeryCount(10),

                                new SurveyModelBuilder()
                                    .WithName("3")
                                    .WithSurgeryCount(100)
                                    .WithSurgeryPrice(100));

        var results = records.Where(expression);

        Assert.Equal(2, results.Count());
        Assert.Contains(results, x => x.Name == "1");
        Assert.Contains(results, x => x.Name == "2");
    }
}

