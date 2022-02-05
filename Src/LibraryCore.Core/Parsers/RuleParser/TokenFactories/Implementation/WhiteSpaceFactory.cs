using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class WhiteSpaceFactory : ITokenFactory
{
    private WhiteSpaceToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked) => char.IsWhiteSpace(characterRead);

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider) => CachedToken;
}

[DebuggerDisplay("Whitespace")]
public record WhiteSpaceToken() : IToken
{
    public Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();
}