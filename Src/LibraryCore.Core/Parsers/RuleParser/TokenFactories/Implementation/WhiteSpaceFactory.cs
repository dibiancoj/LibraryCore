using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class WhiteSpaceFactory : ITokenFactory
{
    private WhiteSpaceToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => char.IsWhiteSpace(characterRead);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine) => CachedToken;
}

[DebuggerDisplay("Whitespace")]
public record WhiteSpaceToken() : IToken
{
    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters) => throw new NotImplementedException();
}