using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class AndAlsoFactory : ITokenFactory
{
    private AndAlsoToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '&' && characterPeaked == '&';

    public Token CreateToken(char characterRead, StringReader stringReader)
    {
        //read the other &
        _ = stringReader.Read();

        return CachedToken;
    }

}

[DebuggerDisplay("&&")]
public record AndAlsoToken() : Token, IBinaryExpressionCombiner
{
    public override Expression CreateExpression(IEnumerable<ParameterExpression> parameters) => throw new NotImplementedException();
}
