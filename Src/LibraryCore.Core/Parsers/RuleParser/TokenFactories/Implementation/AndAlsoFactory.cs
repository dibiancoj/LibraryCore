using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class AndAlsoFactory : ITokenFactory
{
    private AndAlsoToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '&' && characterPeaked == '&';

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the other &
        stringReader.EatXNumberOfCharacters(1);

        return CachedToken;
    }

}

[DebuggerDisplay("&&")]
public record AndAlsoToken() : Token, IBinaryExpressionCombiner
{
    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.AndAlso(left, right);

    public override Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();
}
