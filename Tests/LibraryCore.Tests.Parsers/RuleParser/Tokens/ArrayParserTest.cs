using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Parsers.RuleParser.Fixtures;

namespace LibraryCore.Tests.Parsers.RuleParser.Tokens;

public class ArrayParserTest(RuleParserFixture ruleParserFixture) : IClassFixture<RuleParserFixture>
{
    private RuleParserFixture RuleParserFixture { get; } = ruleParserFixture;

    [Fact]
    public void ArrayOfNumberTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                    .ParseString("[1,2,3]")
                                    .CompilationTokenResult;

        Assert.Single(result);
        Assert.IsType<ArrayToken>(result[0]);

        var array = result.OfType<ArrayToken>().First();
        var values = array.Values.ToArray();

        Assert.IsType<NumberToken<int>>(values[0]);
        Assert.IsType<NumberToken<int>>(values[1]);
        Assert.IsType<NumberToken<int>>(values[2]);
    }

    [Fact]
    public void ArrayOfStringsTest()
    {
        var result = RuleParserFixture.ResolveRuleParserEngine()
                                        .ParseString("['item 1','item 2','item 3']")
                                        .CompilationTokenResult;

        Assert.Single(result);
        Assert.IsType<ArrayToken>(result[0]);

        var array = result.OfType<ArrayToken>().First();
        var values = array.Values.ToArray();

        Assert.IsType<StringToken>(values[0]);
        Assert.IsType<StringToken>(values[1]);
        Assert.IsType<StringToken>(values[2]);
    }

    [Fact]
    public void ArrayInValidSyntax()
    {
        var result = Assert.Throws<Exception>(() => RuleParserFixture.ResolveRuleParserEngine().ParseString("$Id$ == []"));

        Assert.Equal("ArrayFactory Has Blank Array Or Is Not Able To Parse The Value", result.Message);
    }

    //regular doubles
    [InlineData("[1d,2d,3d, 9.99d] contains $Survey.PriceOfSurgery$", true, null, null)]
    [InlineData("[1d,2d,3d] contains $Survey.PriceOfSurgery$", false, null, null)]

    //nullable doubles
    [InlineData("[1d?,2d?,3d?, 9.99d?] contains $Survey.NumberOfBoats$", true, 3d, null)]
    [InlineData("[1d?,2d?,3d?] contains $Survey.NumberOfBoats$", false, null, null)]
    [InlineData("[1d?,2d?,3d?] contains $Survey.NumberOfBoats$", false, 30d, null)]

    //regular ints
    [InlineData("[1,2,3, 10] contains $Survey.SurgeryCount$", true, null, null)]
    [InlineData("[1,2,3] contains $Survey.SurgeryCount$", false, null, null)]

    //nullable ints
    [InlineData("[1?,2?,3?, 5?] contains $Survey.NumberOfMotorcyles$", false, null, 50)]
    [InlineData("[1?,2?,3?] contains $Survey.NumberOfMotorcyles$", true, null, 3)]
    [Theory]
    public void NullableArrayContains(string statementToTest, bool expectedResult, double? numberOfBoats, int? numberOfMotorcycles)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                                .ParseString(statementToTest)
                                .BuildExpression<Survey>("Survey")
                                .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder()
                                                                    .WithNumberOfBoats(numberOfBoats)
                                                                    .WithNumberOfMotorcycles(numberOfMotorcycles)
                                                                    .Value));
    }

    [InlineData("@GetNullableIntArray($Survey$) contains $Survey.NumberOfMotorcyles$", 80, true)]
    [InlineData("@GetNullableIntArray($Survey$) contains $Survey.NumberOfMotorcyles$", 79, false)]
    [Theory]
    public void NullableArrayContainsFromMethod(string statementToTest, int? numberToSet, bool expectedResult)
    {
        var expression = RuleParserFixture.ResolveRuleParserEngine()
                            .ParseString(statementToTest)
                            .BuildExpression<Survey>("Survey")
                            .Compile();

        Assert.Equal(expectedResult, expression.Invoke(new SurveyModelBuilder()
                                                                        .WithNumberOfMotorcycles(numberToSet)
                                                                        .Value));

    }

}

