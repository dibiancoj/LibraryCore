using LibraryCore.Core.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Core.Parsers.RuleParser.TokenFactories.Implementation;

public class AndAlsoFactory : ITokenFactory
{
    private AndAlsoToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => readAndPeakedCharacters == "&&";

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine)
    {
        //read the other &
        RuleParsingUtility.ThrowIfCharacterNotExpected(stringReader, '&');

        return CachedToken;
    }
}

[DebuggerDisplay("&&")]
public record AndAlsoToken() : IToken, IBinaryExpressionCombiner
{
    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.AndAlso(left, right);

    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters) => throw new NotImplementedException();
}
