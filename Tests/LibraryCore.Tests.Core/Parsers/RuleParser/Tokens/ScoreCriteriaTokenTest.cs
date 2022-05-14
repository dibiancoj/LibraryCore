using LibraryCore.Core.Parsers.RuleParser.TokenFactories;
using LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace LibraryCore.Tests.Core.Parsers.RuleParser.Tokens;

public class ScoreCriteriaTokenTest
{
    [Fact]
    public void CreateTokenNotImplemented() => Assert.Throws<NotImplementedException>(() => new ScoreCriteriaToken<int>(10, Array.Empty<IToken>().ToImmutableArray()).CreateExpression(ImmutableList<ParameterExpression>.Empty));
}