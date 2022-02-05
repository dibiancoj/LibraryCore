using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class EqualsFactory : ITokenFactory
{
    private EqualsToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked) => characterRead == '=' && characterPeeked == '=';

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the other equals
        stringReader.EatXNumberOfCharacters(1);
        return CachedToken;
    }
}

[DebuggerDisplay("==")]
public record EqualsToken() : Token, IBinaryComparisonToken
{
    public override Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.Equal(left, right);
}