using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class NullTokenFactory : ITokenFactory
{
    private NullToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => string.Equals(readAndPeakedCharacters, "nu", StringComparison.OrdinalIgnoreCase);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the ull
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'U', 'u');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'L', 'l');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'L', 'l');

        return CachedToken;
    }
}

[DebuggerDisplay("null")]
public record NullToken() : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(null);
}