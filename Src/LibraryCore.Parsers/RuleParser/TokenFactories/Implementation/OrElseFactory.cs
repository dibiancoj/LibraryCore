using LibraryCore.Parsers.RuleParser;
using LibraryCore.Parsers.RuleParser.TokenFactories;
using LibraryCore.Parsers.RuleParser.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;

namespace LibraryCore.Parsers.RuleParser.TokenFactories.Implementation;

public class OrElseFactory : ITokenFactory
{
    private OrElseToken CachedToken { get; } = new();

    public bool IsToken(char characterRead, char characterPeeked, string readAndPeakedCharacters) => readAndPeakedCharacters == "||";

    public IToken CreateToken(char characterRead, StringReader stringReader, TokenFactoryProvider tokenFactoryProvider, RuleParserEngine ruleParserEngine)
    {
        //read the other |
        RuleParsingUtility.EatOrThrowCharacters(stringReader, "|");

        return CachedToken;
    }
}

[DebuggerDisplay("||")]
public record OrElseToken() : IToken, IBinaryExpressionCombiner
{
    public Expression CreateExpression(IImmutableList<ParameterExpression> parameters) => throw new NotImplementedException();

    public Expression CreateBinaryOperatorExpression(Expression left, Expression right) => Expression.OrElse(left, right);
}