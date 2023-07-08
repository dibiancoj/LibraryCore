using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using static LibraryCore.Parsers.RuleParser.RuleParserEngine;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class NullFactory : ITokenFactory
{
    private NullToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => string.Equals(readAndPeakedCharacters, "nu", StringComparison.OrdinalIgnoreCase);

    public IToken CreateToken(char characterRead,
                              StringReader stringReader,
                              CreateTokenParameters createTokenParameters)
    {
        //read the ull
        RuleParsingUtility.EatOrThrowCharacters(stringReader, "ULL");

        return CachedToken;
    }
}

[DebuggerDisplay("null")]
public record NullToken() : IToken
{
    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters) => Expression.Constant(null);
}