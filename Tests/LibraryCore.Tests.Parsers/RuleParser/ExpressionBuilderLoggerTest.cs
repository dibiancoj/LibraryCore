using LibraryCore.Parsers.RuleParser;
using LibraryCore.Parsers.RuleParser.Logging;
using LibraryCore.Tests.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Parsers.RuleParser;

public class ExpressionBuilderLoggerTest : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; }

    public ExpressionBuilderLoggerTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    [InlineData("1 == 2", "(1 == 2)", false)]
    [InlineData("2 == 2", "(2 == 2)", true)]
    [Theory]
    public void NoParameterTest(string stringToParse, string expectedLoggerMessage, bool expectedLoggerResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString(stringToParse)
                                                .BuildExpression()
                                                .WithLogging();
        var logger = new ExpressionLogger();

        Assert.Equal(expectedLoggerResult, expression.Compile().Invoke(logger));
        Assert.Single(logger.LogRecords(), x => x.Message == expectedLoggerMessage && x.Result == expectedLoggerResult);
    }

    [Fact]
    public void NoParameterTestWithMultipleLinesWithTrue()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 == 2 || 2 == 2")
                                                .BuildExpression()
                                                .WithLogging();

        var logger = new ExpressionLogger();

        Assert.True(expression.Compile().Invoke(logger));
        Assert.Equal(3, logger.LogRecords().Count());
        Assert.Contains(logger.LogRecords(), x => x.Message == "((1 == 2) OrElse (2 == 2))" && x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(1 == 2)" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(2 == 2)" && x.Result);
    }

    [Fact]
    public void NoParameterTestWithMultipleLinesWithFalse()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 == 2 && 2 == 3")
                                                .BuildExpression()
                                                .WithLogging();

        var logger = new ExpressionLogger();

        Assert.False(expression.Compile().Invoke(logger));
        Assert.Equal(2, logger.LogRecords().Count()); //2nd half doesn't eval because its aready false
        Assert.Contains(logger.LogRecords(), x => x.Message == "((1 == 2) AndAlso (2 == 3))" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(1 == 2)" && !x.Result);
    }

    [InlineData(5, false)]
    [InlineData(30, true)]
    [Theory]
    public void SimpleParameterTest(int age, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("$Age$ > 15")
                                                .BuildExpression<int>("Age")
                                                .WithLogging()
                                                .Compile();

        var logger = new ExpressionLogger();

        Assert.Equal(expectedResult, expression.Invoke(logger, age));
        Assert.Single(logger.LogRecords());
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Age > 15)" && x.Result == expectedResult);
    }

    [Fact]
    public void SimpleParameterTestWithMultiLineTest1()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("$Age$ > 15 || $Age$ == 5")
                                                .BuildExpression<int>("Age")
                                                .WithLogging()
                                                .Compile();

        var logger = new ExpressionLogger();

        Assert.True(expression.Invoke(logger, 5));
        Assert.Equal(3, logger.LogRecords().Count());
        Assert.Contains(logger.LogRecords(), x => x.Message == "((Age > 15) OrElse (Age == 5))" && x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Age > 15)" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Age == 5)" && x.Result);
    }

    [Fact]
    public void SimpleParameterTestWithMultiLineTest2()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("$Age$ > 15 || $Age$ == 5")
                                                .BuildExpression<int>("Age")
                                                .WithLogging()
                                                .Compile();

        var logger = new ExpressionLogger();

        Assert.True(expression.Invoke(logger, 30));
        Assert.Equal(2, logger.LogRecords().Count());
        Assert.Contains(logger.LogRecords(), x => x.Message == "((Age > 15) OrElse (Age == 5))" && x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Age > 15)" && x.Result);
    }

    [Fact]
    public void TwoParameterTest()
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 25 && $Size.SurgeryCount$ == 12 || $Survey.SurgeryCount$ == 24 && @GetANumberWithNoParameters() == 25")
                                            .BuildExpression<Survey, Survey>("Survey", "Size")
                                            .WithLogging()
                                            .Compile();

        var logger = new ExpressionLogger();

        Assert.False(expression.Invoke(logger,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(24)
                                                .Value,

                                                new SurveyModelBuilder()
                                                .WithSurgeryCount(12)
                                                .Value));

        Assert.Equal(6, logger.LogRecords().Count());
        Assert.Contains(logger.LogRecords(), x => x.Message == "((((Survey.SurgeryCount == 25) AndAlso (Size.SurgeryCount == 12)) OrElse (Survey.SurgeryCount == 24)) AndAlso (GetANumberWithNoParameters() == 25))" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(((Survey.SurgeryCount == 25) AndAlso (Size.SurgeryCount == 12)) OrElse (Survey.SurgeryCount == 24))" && x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "((Survey.SurgeryCount == 25) AndAlso (Size.SurgeryCount == 12))" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Survey.SurgeryCount == 25)" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Survey.SurgeryCount == 24)" && x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(GetANumberWithNoParameters() == 25)" && !x.Result);
    }

    //[Fact]
    //public void TwoParameterTestWithCompilationResult()
    //{
    //    var expression = RuleParserFixture.ResolveRuleParserEngine()
    //                                        .ParseString("$Survey.SurgeryCount$ == 24 && $Size.SurgeryCount$ == 12")
    //                                        .BuildExpression<Survey, Survey>("Survey", "Size")
    //                                        .Compile();

    //    Assert.True(expression.Invoke(new SurveyModelBuilder()
    //                                            .WithSurgeryCount(24)
    //                                            .Value,

    //                                            new SurveyModelBuilder()
    //                                            .WithSurgeryCount(12)
    //                                            .Value));
    //}

    //[Fact]
    //public void ThreeParameterTest()
    //{
    //    var expression = RuleParserFixture.ResolveRuleParserEngine()
    //                                        .ParseString("$Survey.SurgeryCount$ == 30 && $Size.SurgeryCount$ == 12 && $Color.SurgeryCount$ == 15")
    //                                        .BuildExpression<Survey, Survey, Survey>("Survey", "Size", "Color");

    //    Assert.True(expression.Compile().Invoke(new SurveyModelBuilder()
    //                                            .WithSurgeryCount(30)
    //                                            .Value,

    //                                            new SurveyModelBuilder()
    //                                            .WithSurgeryCount(12)
    //                                            .Value,

    //                                            new SurveyModelBuilder()
    //                                            .WithSurgeryCount(15)
    //                                            .Value));
    //}

    //[Fact]
    //public void ThreeParameterTestWithCompilationResult()
    //{
    //    var expression = RuleParserFixture.ResolveRuleParserEngine()
    //                                        .ParseString("$Survey.SurgeryCount$ == 30 && $Size.SurgeryCount$ == 12 && $Color.SurgeryCount$ == 15")
    //                                        .BuildExpression<Survey, Survey, Survey>("Survey", "Size", "Color")
    //                                        .Compile();

    //    Assert.True(expression.Invoke(new SurveyModelBuilder()
    //                                            .WithSurgeryCount(30)
    //                                            .Value,

    //                                            new SurveyModelBuilder()
    //                                            .WithSurgeryCount(12)
    //                                            .Value,

    //                                            new SurveyModelBuilder()
    //                                            .WithSurgeryCount(15)
    //                                            .Value));
    //}

}
