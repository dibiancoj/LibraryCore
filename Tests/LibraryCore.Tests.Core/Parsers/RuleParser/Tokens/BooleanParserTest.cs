using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class BooleanParserTest : IClassFixture<RuleParserFixture>
{
    public BooleanParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [InlineData(false)]
    [InlineData(true)]
    [Theory]
    public void ParseTest(bool canDrive)
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                            .ParseString($"$Survey.CanDrive == {canDrive.ToString().ToLower()}")
                            .CompilationTokenResult;

        Assert.Equal(5, result.Count);
        Assert.IsType<ParameterPropertyToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<BooleanToken>(result[4]);

        var booleanToken = (BooleanToken)result[4];

        Assert.Equal(canDrive, booleanToken.Value);
    }

    [Fact]
    public void TrueStartsValidButEntireWordNotPresent()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.ResolveRuleParserEngine().ParseString("$Id == trNOT"));

        Assert.Equal("Character Read N Is Not Expected. Expected Character = U or u", result.Message);
    }

    //non nullable
    [InlineData("$Survey.CanDrive == true", true, true)]
    [InlineData("$Survey.CanDrive == false", true, false)]
    [InlineData("$Survey.CanDrive == true", false, false)]
    [InlineData("$Survey.CanDrive == false", false, true)]
    [Theory]
    public void ExpressionsToTest(string expressionToTest, bool canDriveToSet, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString(expressionToTest)
                                            .BuildExpression<Survey>("Survey")
                                            .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder()
                                                       .WithCanDrive(canDriveToSet)
                                                       .Value));
    }

    //nullable
    [InlineData("$Survey.HasAccount == true?", true, true)]
    [InlineData("$Survey.HasAccount == false?", true, false)]
    [InlineData("$Survey.HasAccount == true?", false, false)]
    [InlineData("$Survey.HasAccount == false?", false, true)]
    [InlineData("$Survey.HasAccount == false?", null, false)]
    [InlineData("$Survey.HasAccount == true?", null, false)]
    [Theory]
    public void ExpressionsToTestWithNullables(string expressionToTest, bool? hasAccount, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                            .ParseString(expressionToTest)
                                            .BuildExpression<Survey>("Survey")
                                            .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder()
                                                       .WithHasAccount(hasAccount)
                                                       .Value));
    }

}

