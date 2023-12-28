using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class GreaterThenFactory : ITokenFactory
{
    private GreaterThenToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => characterRead == '>' && characterPeeked != '=';

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              CreateTokenParameters createTokenParameters) => CachedToken;
}

[DebuggerDisplay(">")]
public record GreaterThenToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateExpression(IReadOnlyList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.GreaterThan(left, right);
}
