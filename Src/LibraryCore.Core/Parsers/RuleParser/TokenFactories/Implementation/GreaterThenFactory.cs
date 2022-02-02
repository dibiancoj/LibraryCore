using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class GreaterThenFactory : ITokenFactory
{
    private GreaterThenToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '>' && characterPeaked != '=';

    public Token CreateToken(char characterRead, StringReader stringReader) => CachedToken;

}

[DebuggerDisplay(">")]
public record GreaterThenToken() : Token, IBinaryComparisonToken
{
    public override Expression CreateExpression(ParameterExpression surveyParameter) => throw new NotImplementedException();
}
