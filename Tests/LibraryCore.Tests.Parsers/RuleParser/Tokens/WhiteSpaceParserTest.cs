using LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Parsers.RuleParser.Tokens;

public class WhiteSpaceParserTest
{
    [Fact]
    public void CreateTokenNotImplemented() => Assert.Throws<NotImplementedException>(() => new WhiteSpaceToken().CreateExpression(ImmutableList<ParameterExpression>.Empty));
}