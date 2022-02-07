using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class FalseFactory : ITokenFactory
{
    private FalseToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => string.Equals(readAndPeakedCharacters, "fa", StringComparison.OrdinalIgnoreCase);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read f ...alse
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'A', 'a');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'L', 'l');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'S', 's');
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, 'E', 'e');

        return CachedToken;
    }
}

[DebuggerDisplay("False")]
public record FalseToken() : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(false);
}