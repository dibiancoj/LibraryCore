using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class LikeFactory : ITokenFactory
{
    private LikeToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked) => characterRead == 'l' && characterPeeked == 'i';

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the l...ike
        stringReader.EatXNumberOfCharacters(3);
        return CachedToken;
    }
}

[DebuggerDisplay("Like")]
public record LikeToken() : Token, IBinaryComparisonToken
{
    public override Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right)
    {
        var stringContains = typeof(string).GetMethods()
                                           .First(x => x.Name == nameof(string.Contains) && x.GetParameters().Length == 1);

        return Expression.Call(left, stringContains, right);
    }
}