using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class TrueFactory : ITokenFactory
{
    private TrueToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => string.Equals(new string(new char[] { characterRead, characterPeaked }), "tr", StringComparison.OrdinalIgnoreCase);

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //rue alse
        stringReader.EatXNumberOfCharacters(3);
        
        return CachedToken;
    }
}

[DebuggerDisplay("True")]
public record TrueToken() : Token
{
    public override Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(true);
}