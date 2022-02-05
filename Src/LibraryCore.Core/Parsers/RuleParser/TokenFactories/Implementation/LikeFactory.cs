using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class LikeFactory : ITokenFactory
{
    private LikeToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => string.Equals(readAndPeakedCharacters, "li", StringComparison.OrdinalIgnoreCase);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the l...ike
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'I', 'i');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'K', 'k');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'E', 'e');
        return CachedToken;
    }
}

[DebuggerDisplay("Like")]
public record LikeToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right)
    {
        var stringContains = typeof(string).GetMethods()
                                           .First(x => x.Name == nameof(string.Contains) && x.GetParameters().Length == 1);

        return Expression.Call(left, stringContains, right);
    }
}