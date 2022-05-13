using LibraryCore.Core.Parsers.RuleParser.Utilities;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class NullFactory : ITokenFactory
{
    private NullToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => string.Equals(readAndPeakedCharacters, "nu", StringComparison.OrdinalIgnoreCase);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine)
    {
        //read the ull
        RuleParsingUtility.EatOrThrowCharacters(stringReader, "ULL");

        return CachedToken;
    }
}

[DebuggerDisplay("null")]
public record NullToken() : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(null);
}