using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class WhiteSpaceParserTest
{
    [Fact]
    public void CreateTokenNotImplemented() => Assert.Throws<NotImplementedException>(() => new WhiteSpaceToken().CreateExpression(Array.Empty<ParameterExpression>()));
}