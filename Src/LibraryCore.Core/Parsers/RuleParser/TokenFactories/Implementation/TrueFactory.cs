using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class TrueFactory : ITokenFactory
{
    private TrueToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => string.Equals(readAndPeakedCharacters, "tr", StringComparison.OrdinalIgnoreCase);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the t ..rue
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'R', 'r');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'U', 'u');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'E', 'e');

        return CachedToken;
    }
}

[DebuggerDisplay("True")]
public record TrueToken() : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(true);
}