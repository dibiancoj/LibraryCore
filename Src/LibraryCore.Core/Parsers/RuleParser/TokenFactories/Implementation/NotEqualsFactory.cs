using LibraryCore.Core.ExtensionMethods;
using LibraryCore.Core.Parsers.RuleParser.Utilities;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class NotEqualsFactory : ITokenFactory
{
    private NotEqualsToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => readAndPeakedCharacters == "!=";

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the other equals
        RuleParsingUtility.EatOrThrowCharacters(stringReader, "=");

        return CachedToken;
    }
}

[DebuggerDisplay("!=")]
public record NotEqualsToken() : IToken, IBinaryComparisonToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.NotEqual(left, right);
}