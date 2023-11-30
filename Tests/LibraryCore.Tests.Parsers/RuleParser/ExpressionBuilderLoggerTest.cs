using LibraryCore.Parsers.RuleParser;
using LibraryCore.Parsers.RuleParser.Logging;
using LibraryCore.Tests.Parsers.RuleParser.Fixtures;
using System.Text.Json;

namespace LibraryCore.Tests.Parsers.RuleParser;

public class ExpressionBuilderLoggerTest(RuleParserFixture ruleParserFixture) : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; } = ruleParserFixture;

    [InlineData("1 == 2", "(1 == 2)", false, false)]
    [InlineData("2 == 2", "(2 == 2)", true, false)]
    [InlineData("1 == 2", "(1 == 2)", false, true)]
    [InlineData("2 == 2", "(2 == 2)", true, true)]
    [Theory]
    public void NoParameterTest(string stringToParse, string expectedLoggerMessage, bool expectedLoggerResult, bool withParameterDebugging)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString(stringToParse)
                                                .BuildExpression()
                                                .WithLogging(withParameterDebugging);
        var logger = new ExpressionLogger();

        Assert.Equal(expectedLoggerResult, expression.Compile().Invoke(logger));
        Assert.Single(logger.LogRecords(), x => x.Message == expectedLoggerMessage && x.Result == expectedLoggerResult);
        Assert.Empty(logger.ParameterRecords());
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void NoParameterTestWithMultipleLinesWithTrue(bool withParameterDebugging)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 == 2 || 2 == 2")
                                                .BuildExpression()
                                                .WithLogging(withParameterDebugging);

        var logger = new ExpressionLogger();

        Assert.True(expression.Compile().Invoke(logger));
        Assert.Equal(3, logger.LogRecords().Count());
        Assert.Contains(logger.LogRecords(), x => x.Message == "((1 == 2) OrElse (2 == 2))" && x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(1 == 2)" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(2 == 2)" && x.Result);
        Assert.Empty(logger.ParameterRecords());
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void NoParameterTestWithMultipleLinesWithFalse(bool withParameterDebugging)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("1 == 2 && 2 == 3")
                                                .BuildExpression()
                                                .WithLogging(withParameterDebugging);

        var logger = new ExpressionLogger();

        Assert.False(expression.Compile().Invoke(logger));
        Assert.Equal(2, logger.LogRecords().Count()); //2nd half doesn't eval because its aready false
        Assert.Contains(logger.LogRecords(), x => x.Message == "((1 == 2) AndAlso (2 == 3))" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(1 == 2)" && !x.Result);
        Assert.Empty(logger.ParameterRecords());
    }

    [InlineData(5, false, false)]
    [InlineData(30, false, true)]
    [InlineData(5, true, false)]
    [InlineData(30, true, true)]
    [Theory]
    public void SimpleParameterTest(int age, bool withParameterDebugging, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("$Age$ > 15")
                                                .BuildExpression<int>("Age")
                                                .WithLogging(withParameterDebugging)
                                                .Compile();

        var logger = new ExpressionLogger();

        Assert.Equal(expectedResult, expression.Invoke(logger, age));
        Assert.Single(logger.LogRecords());
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Age > 15)" && x.Result == expectedResult);

        if (withParameterDebugging)
        {
            Assert.Single(logger.ParameterRecords());
            Assert.Single(logger.ParameterRecords(), x => x.Name == "Age" && x.Value == age.ToString());
        }
        else
        {
            Assert.Empty(logger.ParameterRecords());
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void SimpleParameterTestWithMultiLineTest1(bool withParameterDebugging)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("$Age$ > 15 || $Age$ == 5")
                                                .BuildExpression<int>("Age")
                                                .WithLogging(withParameterDebugging: withParameterDebugging)
                                                .Compile();

        var logger = new ExpressionLogger();

        Assert.True(expression.Invoke(logger, 5));
        Assert.Equal(3, logger.LogRecords().Count());
        Assert.Contains(logger.LogRecords(), x => x.Message == "((Age > 15) OrElse (Age == 5))" && x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Age > 15)" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Age == 5)" && x.Result);

        if (withParameterDebugging)
        {
            Assert.Single(logger.ParameterRecords());
            Assert.Contains(logger.ParameterRecords(), x => x.Name == "Age" && x.Value == "5");
        }
        else
        {
            Assert.Empty(logger.ParameterRecords());
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void SimpleParameterTestWithMultiLineTest2(bool withParameterDebugging)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("$Age$ > 15 || $Age$ == 5")
                                                .BuildExpression<int>("Age")
                                                .WithLogging(withParameterDebugging)
                                                .Compile();

        var logger = new ExpressionLogger();

        const int age = 30;

        Assert.True(expression.Invoke(logger, age));
        Assert.Equal(2, logger.LogRecords().Count());
        Assert.Contains(logger.LogRecords(), x => x.Message == "((Age > 15) OrElse (Age == 5))" && x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Age > 15)" && x.Result);

        if (withParameterDebugging)
        {
            Assert.Single(logger.ParameterRecords());
            Assert.Contains(logger.ParameterRecords(), x => x.Name == "Age" && x.Value == age.ToString());
        }
        else
        {
            Assert.Empty(logger.ParameterRecords());
        }
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void TwoParameterTest(bool withParameterDebugging)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString("$Survey.SurgeryCount$ == 25 && $Size.SurgeryCount$ == 12 || $Survey.SurgeryCount$ == 24 && @GetANumberWithNoParameters() == 25")
                                            .BuildExpression<Survey, Survey>("Survey", "Size")
                                            .WithLogging(withParameterDebugging)
                                            .Compile();

        var logger = new ExpressionLogger();

        var parameter1 = new SurveyModelBuilder()
                                       .WithSurgeryCount(24)
                                       .Value;

        var parameter2 = new SurveyModelBuilder()
                                       .WithSurgeryCount(12)
                                       .Value;

        Assert.False(expression.Invoke(logger, parameter1, parameter2));

        Assert.Equal(6, logger.LogRecords().Count());
        Assert.Contains(logger.LogRecords(), x => x.Message == "((((Survey.SurgeryCount == 25) AndAlso (Size.SurgeryCount == 12)) OrElse (Survey.SurgeryCount == 24)) AndAlso (GetANumberWithNoParameters() == 25))" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(((Survey.SurgeryCount == 25) AndAlso (Size.SurgeryCount == 12)) OrElse (Survey.SurgeryCount == 24))" && x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "((Survey.SurgeryCount == 25) AndAlso (Size.SurgeryCount == 12))" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Survey.SurgeryCount == 25)" && !x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(Survey.SurgeryCount == 24)" && x.Result);
        Assert.Contains(logger.LogRecords(), x => x.Message == "(GetANumberWithNoParameters() == 25)" && !x.Result);

        if (withParameterDebugging)
        {
            var jsonSerializer = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };

            Assert.Equal(2, logger.ParameterRecords().Count());

            var parameterResults = logger.ParameterRecords().ToList();

            Assert.Equal("Survey", parameterResults[0].Name);
            Assert.Equal(JsonSerializer.Serialize(parameter1, jsonSerializer), parameterResults[0].Value, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);

            Assert.Equal("Size", parameterResults[1].Name);
            Assert.Equal(JsonSerializer.Serialize(parameter2, jsonSerializer), parameterResults[1].Value, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }
        else
        {
            Assert.Empty(logger.ParameterRecords());
        }
    }

}
