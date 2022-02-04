using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class FalseFactory : ITokenFactory
{
    private FalseToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => string.Equals(new string(new char[] { characterRead, characterPeaked }), "fa", StringComparison.OrdinalIgnoreCase);

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read f ...alse
        stringReader.EatXNumberOfCharacters(4);

        return CachedToken;
    }
}

[DebuggerDisplay("False")]
public record FalseToken() : Token
{
    public override Expression CreateExpression(IList<ParameterExpression> parameters) => Expression.Constant(false);
}