using LibraryCore.Core.ExtensionMethods;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class OrElseFactory : ITokenFactory
{
    private OrElseToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeaked) => characterRead == '|' && characterPeaked == '|';

    public Token CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider)
    {
        //read the other ||
        stringReader.EatXNumberOfCharacters(1);
        return CachedToken;
    }
}

[DebuggerDisplay("||")]
public record OrElseToken() : Token, IBinaryExpressionCombiner
{
    public override Expression CreateExpression(IList<ParameterExpression> parameters) => throw new NotImplementedException();
}