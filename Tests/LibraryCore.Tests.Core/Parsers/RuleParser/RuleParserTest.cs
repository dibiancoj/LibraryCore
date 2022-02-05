using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using LibraryCore.Tests.Core.Parsers.RuleParser.Fixtures;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers.RuleParser;

public class RuleParserTest : IClassFixture<RuleParserFixture>
{
    public RuleParserTest(RuleParserFixture ruleParserFixture)
    {
        RuleParserFixture = ruleParserFixture;
    }

    private RuleParserFixture RuleParserFixture { get; }

    #region Basic Factory Test Parsing

    [Fact]
    public void NoFactoryFoundTest()
    {
        var exceptionThrown = Assert.Throws<Exception>(() =>
        {
            _ = RuleParserFixture.RuleParserEngineToUse.ParseString("true ** true");
        });

        Assert.Contains("No Token Found For Value = **", exceptionThrown.Message);
    }

    [Theory]
    [InlineData("True")]
    [InlineData("true")]
    [InlineData("TRUE")]
    public void TruthFactoryTest(string trueCase)
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString($"{trueCase} == {trueCase}");

        Assert.Equal(5, result.Count);
        Assert.IsType<TrueToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<TrueToken>(result[4]);
    }

    [Theory]
    [InlineData("False")]
    [InlineData("false")]
    [InlineData("FALSE")]
    public void FalseFactoryTest(string falseCase)
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString($"{falseCase} == {falseCase}");

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
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("'abc' == 'def'");

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
    public void NullFactoryTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 == null");

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NullToken>(result[4]);
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
    public void LikeTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("'test' like 'tester'");

        Assert.Equal(5, result.Count);
        Assert.IsType<StringToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<LikeToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<StringToken>(result[4]);
    }

    [Fact]
    public void NotEqualTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("1 != 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<NumberToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<NotEqualsToken>(result[2]);
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

    [Fact]
    public void ParameterPropertyNameWithParameterNameTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("$Survey.PatientName == 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<ParameterPropertyToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken>(result[4]);
    }

    [Fact]
    public void MethodCallTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("@MyMethod1(1,true, 'bla') == 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<MethodCallToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<EqualsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken>(result[4]);
    }

    [Fact]
    public void ContainsTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("@MyMethod1() contains 1");

        Assert.Equal(5, result.Count);
        Assert.IsType<MethodCallToken>(result[0]);
        Assert.IsType<WhiteSpaceToken>(result[1]);
        Assert.IsType<ContainsToken>(result[2]);
        Assert.IsType<WhiteSpaceToken>(result[3]);
        Assert.IsType<NumberToken>(result[4]);
    }

    [Fact]
    public void ArrayOfNumberTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("[1,2,3]");

        Assert.Equal(1, result.Count);
        Assert.IsType<ArrayToken>(result[0]);

        var array = result.OfType<ArrayToken>().First();
        var values = array.Values.ToArray();

        Assert.IsType<NumberToken>(values[0]);
        Assert.IsType<NumberToken>(values[1]);
        Assert.IsType<NumberToken>(values[2]);
    }

    [Fact]
    public void ArrayOfStringsTest()
    {
        var result = RuleParserFixture.RuleParserEngineToUse.ParseString("['item 1','item 2','item 3']");

        Assert.Equal(1, result.Count);
        Assert.IsType<ArrayToken>(result[0]);

        var array = result.OfType<ArrayToken>().First();
        var values = array.Values.ToArray();

        Assert.IsType<StringToken>(values[0]);
        Assert.IsType<StringToken>(values[1]);
        Assert.IsType<StringToken>(values[2]);
    }

    #endregion

    #region Create Token - Not Implemented

    [Fact]
    public void EqualCreateTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new EqualsToken().CreateExpression(Array.Empty<ParameterExpression>()));

    [Fact]
    public void NotEqualCreateTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new NotEqualsToken().CreateExpression(Array.Empty<ParameterExpression>()));

    [Fact]
    public void AndAlsoCreateTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new AndAlsoToken().CreateExpression(Array.Empty<ParameterExpression>()));

    [Fact]
    public void OrElseCreateTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new OrElseToken().CreateExpression(Array.Empty<ParameterExpression>()));

    [Fact]
    public void WhiteSpaceCreateTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new WhiteSpaceToken().CreateExpression(Array.Empty<ParameterExpression>()));

    [Fact]
    public void LessThenTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new LessThenToken().CreateExpression(Array.Empty<ParameterExpression>()));

    [Fact]
    public void LessThenOrEqualTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new LessThenOrEqualToken().CreateExpression(Array.Empty<ParameterExpression>()));

    [Fact]
    public void GreaterThenTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new GreaterThenToken().CreateExpression(Array.Empty<ParameterExpression>()));

    [Fact]
    public void GreaterThenEqualTokenNotImplement() => Assert.Throws<NotImplementedException>(() => new GreaterThenOrEqualToken().CreateExpression(Array.Empty<ParameterExpression>()));

    #endregion

}