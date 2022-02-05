using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class GreaterThenFactory : ITokenFactory
{
    private GreaterThenToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked) => characterRead == '>' && characterPeeked != '=';

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider) => CachedToken;
}

[DebuggerDisplay(">")]
public record GreaterThenToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.GreaterThan(left, right);
}
