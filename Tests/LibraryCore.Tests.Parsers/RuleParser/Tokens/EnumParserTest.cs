using LibraryCore.Parsers.RuleParser;
using LibraryCore.Parsers.RuleParser.Logging;
using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Parsers.RuleParser.Fixtures;
using static LibraryCore.Tests.Parsers.RuleParser.Fixtures.Survey;

namespace LibraryCore.Tests.Parsers.RuleParser.Tokens;

public class EnumParserTest(RuleParserFixture ruleParserFixture) : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; } = ruleParserFixture;

    [Fact]
    public void ParseTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString("$Survey.SmokingStatus$ == #LibraryCore.Tests.Parsers.RuleParser.Fixtures.Survey+SmokingStatusEnum|SmokeAlot#")
                                                .CompilationTokenResult;

        Assert.Equal(5, result.Count);
        Assert.IsType<ParameterPropertyToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<EnumToken>(result[4]);

        var enumToken = result.OfType<EnumToken>().Last();

        Assert.Equal(Survey.SmokingStatusEnum.SmokeAlot.ToString(), enumToken.Value);
    }

    [Fact]
    public void StringWithNoClosingBracket()
    {
        _ = Assert.Throws<Exception>(() => RuleParserFixture.ResolveRuleParserEngine().ParseString("$Survey.SmokingStatus$ == #noclosingbracket"));
    }

    [InlineData(true, SmokingStatusEnum.SmokeAlot)]
    [InlineData(false, SmokingStatusEnum.Moderate)]
    [InlineData(false, SmokingStatusEnum.DoNotSmoke)]
    [Theory]
    public void EqualExpression(bool expectedResult, SmokingStatusEnum smokingStatusValueToSet)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString($"$Survey.SmokingStatus$ == #LibraryCore.Tests.Parsers.RuleParser.Fixtures.Survey+SmokingStatusEnum|{smokingStatusValueToSet}#")
                                                .BuildExpression<Survey>("Survey")
                                                .WithLogging(true)
                                                .Compile();

        var logger = new ExpressionLogger();

        var parameter1 = new SurveyModelBuilder()
                                       .WithSmokingStatus(SmokingStatusEnum.SmokeAlot)
                                       .Value;

        var result = expression.Invoke(logger, parameter1);

        Assert.Equal(expectedResult, result);
    }

    [InlineData(true, SmokingStatusEnum.SmokeAlot)]
    [InlineData(false, SmokingStatusEnum.Moderate)]
    [InlineData(false, SmokingStatusEnum.DoNotSmoke)]
    [Theory]
    public void EqualExpressionWithNullable(bool expectedResult, SmokingStatusEnum smokingStatusValueToSet)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                                .ParseString($"$Survey.NullableSmokingStatus$ == #LibraryCore.Tests.Parsers.RuleParser.Fixtures.Survey+SmokingStatusEnum|{smokingStatusValueToSet}?#")
                                                .BuildExpression<Survey>("Survey")
                                                .WithLogging(true)
                                                .Compile();

        var logger = new ExpressionLogger();

        var parameter1 = new SurveyModelBuilder()
                                       .WithNullableSmokingStatus(SmokingStatusEnum.SmokeAlot)
                                       .Value;

        var result = expression.Invoke(logger, parameter1);

        Assert.Equal(expectedResult, result);
    }

}

