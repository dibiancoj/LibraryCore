using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class EqualsFactory : ITokenFactory
{
    private EqualsToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '=' && characterPeaked == '=';

    public Token CreateToken(char characterRead, StringReader stringReader)
    {
        //read the other equals
        _ = stringReader.Read();
        return CachedToken;
    }
}

[DebuggerDisplay("==")]
public record EqualsToken() : Token, IBinaryComparisonToken
{
    public override Expression CreateExpression(ParameterExpression surveyParameter) => throw new NotImplementedException();
}