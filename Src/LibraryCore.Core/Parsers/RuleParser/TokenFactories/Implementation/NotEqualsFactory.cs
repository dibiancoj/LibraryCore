using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class NotEqualsFactory : ITokenFactory
{
    private NotEqualsToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked) => characterRead == '!' && characterPeeked == '=';

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the other equals
        stringReader.EatXNumberOfCharacters(1);

        return CachedToken;
    }
}

[DebuggerDisplay("!=")]
public record NotEqualsToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.NotEqual(left, right);
}