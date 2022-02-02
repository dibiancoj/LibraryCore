using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class FalseFactory : ITokenFactory
{
    private FalseToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == 'f' && characterPeaked == 'a';

    public Token CreateToken(char characterRead, StringReader stringReader)
    {
        //read alse
        _ = stringReader.Read();
        _ = stringReader.Read();
        _ = stringReader.Read();
        _ = stringReader.Read();
        return CachedToken;
    }

}

[DebuggerDisplay("False")]
public record FalseToken() : Token
{
    public override Expression CreateExpression(IEnumerable<ParameterExpression> parameters) => Expression.Constant(false);
}