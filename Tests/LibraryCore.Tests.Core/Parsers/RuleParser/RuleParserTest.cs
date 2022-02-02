using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class RuleParserTest : IClassFixture<RuleParserFixture>
{
    public RuleParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    [Fact]
    public void NoFactoryFoundTest()
    {
        var exceptionThrown = Assert.Throws<Exception>(() =>
        {
            _ = RuleParserFixture.RuleParserEngineToUse.ParseString("true ** true");
        });

        Assert.Contains("No Token Found For Value = **", exceptionThrown.Message);
    }

    [Fact]
    public void TruthFactoryTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("true == true");

        Assert.Equal(5, result.Count);
        Assert.IsType<TrueToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<TrueToken>(result[4]);
    }

    [Fact]
    public void FalseFactoryTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("false == false");

        Assert.Equal(5, result.Count);
        Assert.IsType<FalseToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<FalseToken>(result[4]);
    }

    [Fact]
    public void StringFactoryTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("[abc] == [def]");

        Assert.Equal(5, result.Count);
        Assert.IsType<StringToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<StringToken>(result[4]);
    }

    [Fact]
    public void NumberFactoryTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 == 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken>(result[4]);
    }

    [Fact]
    public void LessThenTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 < 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<LessThenToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken>(result[4]);
    }

    [Fact]
    public void LessThenOrEqualTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 <= 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<LessThenOrEqualToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken>(result[4]);
    }

    [Fact]
    public void GreatThenTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 > 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<GreaterThenToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken>(result[4]);
    }

    [Fact]
    public void GreatThenOrEqualTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 >= 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<GreaterThenOrEqualToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken>(result[4]);
    }

    [Fact]
    public void EqualTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 == 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken>(result[4]);
    }

    [Fact]
    public void AndAlsoTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 == 1 && 2 == 2");

        Assert.Equal(13, result.Count);
        Assert.IsType<NumberToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken>(result[4]);

        Assert.IsType<WhiteSpaceToken>(result[5]);
        Assert.IsType<AndAlsoToken>(result[6]);
        Assert.IsType<WhiteSpaceToken>(result[7]);

        Assert.IsType<NumberToken>(result[8]);
        Assert.IsType<WhiteSpaceToken>(result[9]);
        Assert.IsType<EqualsToken>(result[10]);
        Assert.IsType<WhiteSpaceToken>(result[11]);
        Assert.IsType<NumberToken>(result[12]);
    }

    [Fact]
    public void OrElseTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 == 1 || 2 == 2");

        Assert.Equal(13, result.Count);
        Assert.IsType<NumberToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken>(result[4]);

        Assert.IsType<WhiteSpaceToken>(result[5]);
        Assert.IsType<OrElseToken>(result[6]);
        Assert.IsType<WhiteSpaceToken>(result[7]);

        Assert.IsType<NumberToken>(result[8]);
        Assert.IsType<WhiteSpaceToken>(result[9]);
        Assert.IsType<EqualsToken>(result[10]);
        Assert.IsType<WhiteSpaceToken>(result[11]);
        Assert.IsType<NumberToken>(result[12]);
    }

}