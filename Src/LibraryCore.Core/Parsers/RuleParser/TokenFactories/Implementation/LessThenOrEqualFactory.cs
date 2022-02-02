using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class LessThenOrEqualFactory : ITokenFactory
{
    private LessThenOrEqualToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '<' && characterPeaked == '=';

    public Token CreateToken(char characterRead, StringReader stringReader)
    {
        //read the last =
        _ = stringReader.Read();
        return CachedToken;
    }

}

[DebuggerDisplay("<=")]
public record LessThenOrEqualToken() : Token, IBinaryComparisonToken
{
    public override Expression CreateExpression(ParameterExpression surveyParameter) => throw new NotImplementedException();
}