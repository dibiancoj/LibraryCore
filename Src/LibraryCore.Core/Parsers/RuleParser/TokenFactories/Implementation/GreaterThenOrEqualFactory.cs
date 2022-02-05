using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class GreaterThenOrEqualFactory : ITokenFactory
{
    private GreaterThenOrEqualToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked) => characterRead == '>' && characterPeeked == '=';

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the last =
        stringReader.EatXNumberOfCharacters(1);

        return CachedToken;
    }
}

[DebuggerDisplay(">=")]
public record GreaterThenOrEqualToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.GreaterThanOrEqual(left, right);
}