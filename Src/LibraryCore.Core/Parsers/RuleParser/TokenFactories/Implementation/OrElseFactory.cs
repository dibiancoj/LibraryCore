using LibraryCore.Core.Parsers.RuleParser.Utilities;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class OrElseFactory : ITokenFactory
{
    private OrElseToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => readAndPeakedCharacters == "||";

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the other |
        RuleParsingUtility.EatOrThrowCharacters(stringReader, "|");

        return CachedToken;
    }
}

[DebuggerDisplay("||")]
public record OrElseToken() : IToken, IBinaryExpressionCombiner
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.OrElse(left, right);
}