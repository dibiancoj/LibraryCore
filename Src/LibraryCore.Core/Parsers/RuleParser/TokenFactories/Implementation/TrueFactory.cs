using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class TrueFactory : ITokenFactory
{
    private TrueToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == 't' && characterPeaked == 'r';

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //rue alse
        _ = stringReader.Read();
        _ = stringReader.Read();
        _ = stringReader.Read();
        
        return CachedToken;
    }
}

[DebuggerDisplay("True")]
public record TrueToken() : Token
{
    public override Expression CreateExpression(IEnumerable<ParameterExpression> parameters) => Expression.Constant(true);
}