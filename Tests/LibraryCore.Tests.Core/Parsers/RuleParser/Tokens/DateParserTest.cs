using LibraryCore.Core.Parsers.RuleParser;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class DateParserTest : IClassFixture<RuleParserFixture>
{
    public DateParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void ParseTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("$Param1 == ^1/1/2020^");

        Assert.Equal(5, result.Count);
        Assert.IsType<ParameterPropertyToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<DateToken>(result[4]);
        Assert.Equal(typeof(DateTime), ((DateToken)result[4]).TypeToUse);
    }

    [Fact]
    public void DateTimeWithTimeParseTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("$Param1 == ^1/1/2020 2:00pm^");

        Assert.Equal(5, result.Count);
        Assert.IsType<ParameterPropertyToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<DateToken>(result[4]);

        var dateAsDateToken = (DateToken)result[4];

        Assert.Equal(typeof(DateTime), dateAsDateToken.TypeToUse);
        Assert.Equal(new DateTime(2020, 1, 1, 14, 0, 0), dateAsDateToken.Value);
    }

    [Fact]
    public void NullableDateTimeParseTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("$Param1 == ^1/1/2020^?");

        Assert.Equal(5, result.Count);
        Assert.IsType<ParameterPropertyToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<DateToken>(result[4]);
        Assert.Equal(typeof(DateTime?), ((DateToken)result[4]).TypeToUse);
    }

    [InlineData("$Survey.DateOfBirth == ^1/1/2020^", false)]
    [InlineData("$Survey.DateOfBirth >= ^1/1/2020^", true)]
    [InlineData("$Survey.DateOfBirth == ^3/1/2020^", false)]
    [InlineData("$Survey.DateOfBirth == ^2/1/2020^", true)]
    [InlineData("$Survey.DateOfBirth < ^1/1/2019^", false)]
    [InlineData("$Survey.DateOfBirth > ^1/1/2019^", true)]
    [Theory]
    public void DateTimeTests(string clauseToTest, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(clauseToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder().Value));
    }

    [InlineData("$Survey.LastLogin == ^2/1/2020^?", true)]
    [InlineData("$Survey.LastLogin == ^1/1/2020^?", false)]
    [InlineData("$Survey.LastLogin >= ^1/1/2020^?", true)]
    [InlineData("$Survey.LastLogin == ^3/1/2020^?", false)]
    [InlineData("$Survey.LastLogin < ^1/1/2019^?", false)]
    [InlineData("$Survey.LastLogin > ^1/1/2019^?", true)]
    [Theory]
    public void NullableDateTimeTests(string clauseToTest, bool expectedResult)
    {
        var tokens = RuleParserFixture.RuleParserEngineToUse.ParseString(clauseToTest);
        var expression = RuleParserExpressionBuilder.BuildExpression<Survey>(tokens, "Survey");

        Assert.Equal(expectedResult, expression.Compile().Invoke(new SurveyModelBuilder()
                                                                    .WithLastLogIn(new DateTime(2020, 2, 1))
                                                                    .Value));
    }

}

