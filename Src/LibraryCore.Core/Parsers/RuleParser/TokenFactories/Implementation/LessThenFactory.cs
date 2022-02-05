using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class LessThenFactory : ITokenFactory
{
    private LessThenToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked) => characterRead == '<' && characterPeeked != '=';

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider) => CachedToken;
}

[DebuggerDisplay("<")]
public record LessThenToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.LessThan(left, right);
}
